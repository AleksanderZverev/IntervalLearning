using System.Globalization;
using DB;
using DB.Models;
using IntervalLearningApi.Models;
using IntervalLearningApi.Services.Jwt;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace IntervalLearningApi.Services.Authentication;

public class AuthenticationService : IAuthenticationService
{
    private readonly ApplicationContext db;
    private readonly IJwtService jwtService;
    private readonly IWebHostEnvironment environment;
    private readonly JwtSettings jwtSettings;

    public AuthenticationService(
        ApplicationContext db,
        IJwtService jwtService,
        IOptions<JwtSettings> jwtSettings,
        IWebHostEnvironment environment)
    {
        this.db = db;
        this.jwtService = jwtService;
        this.environment = environment;
        this.jwtSettings = jwtSettings.Value;
    }

    public (bool ok, string? error) Register(RegisterRequest request, string sourceIpAddress)
    {
        var emailLower = request.Email.ToLowerInvariant();
        var sameUser = db.Users.FirstOrDefault(u => u.Email == emailLower);

        if (sameUser != null)
            return (false, "Email already exists");

        var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

        var user = new UserEntity
        {
            Email = emailLower,
            PasswordHash = new UserPasswordsEntity()
            {
                PasswordHash = passwordHash,
            },
            FirstName = request.FirstName,
            LastName = request.LastName ?? "",
        };
        
        try
        {
            //TODO: TEST
            db.Database.BeginTransaction();

            db.Users.Add(user);
            db.SaveChanges();

            var metadata = new UserMetadataEntity(user.Id);
            db.Entry(metadata).State = EntityState.Added;

            db.SaveChanges();
            db.Database.CommitTransaction();
        }
        catch
        {
            return (false, "Unknown error");
        }

        return (true, null);
    }

    public (AuthenticateResponse? response, string? errorMessage) Authenticate(AuthenticateRequest req, string ipAddress)
    {
        var user = db.Users.Include(u => u.PasswordHash).Include(u => u.RefreshTokens).SingleOrDefault(x => x.Email == req.Email);

        if (user is {PasswordHash: null})
            return (null, "Not signed up user!");

        if (user == null || !BCrypt.Net.BCrypt.Verify(req.Password, user.PasswordHash.PasswordHash))
            return (null, "Email or password is incorrect");

        return (Authenticate(user, ipAddress), null);
    }

    public AuthenticateResponse? TryAuthenticateByOldToken(string jwtToken, string refreshToken)
    {
        var userId = jwtService.ValidateJwtToken(jwtToken, DateTime.UtcNow.AddMinutes(5));

        if (userId == null)
            return null;

        var user = db.Users.Single(u => u.Id == userId);
        return new AuthenticateResponse(user, jwtToken, refreshToken);
    }

    private AuthenticateResponse Authenticate(UserEntity user, string ipAddress)
    {
        var jwtToken = jwtService.GenerateJwtToken(user);
        var refreshToken = jwtService.GenerateRefreshToken(user, ipAddress);

        user.RefreshTokens.Add(refreshToken);

        RemoveOldRefreshTokens(user);
        db.SaveChanges();

        return new AuthenticateResponse(user, jwtToken, refreshToken.Token);
    }

    public (AuthenticateResponse? response, string? error) RefreshToken(string refreshToken, string ipAddress)
    {
        var (user, userError) = GetUserByRefreshToken(refreshToken);

        if (user == null)
            return (null, userError);

        var refreshTokenItem = user.RefreshTokens.Single(x => x.Token == refreshToken);

        if (refreshTokenItem.IsRevoked)
        {
            RevokeDescendantRefreshTokens(refreshTokenItem, user, ipAddress, $"Attempted reuse of revoked ancestor token: {refreshToken}");
            db.Update(user);
            db.SaveChanges();
        }

        if (!refreshTokenItem.IsActive)
            return (null, "Invalid token");

        var newRefreshToken = ReplaceOldRefreshToken(user, refreshTokenItem, ipAddress);
        user.RefreshTokens.Add(newRefreshToken);
        
        RemoveOldRefreshTokens(user);
        db.SaveChanges();

        var jwtToken = jwtService.GenerateJwtToken(user);

        return (new AuthenticateResponse(user, jwtToken, newRefreshToken.Token), null);
    }

    public (bool ok, string? error) RevokeToken(string token, string ipAddress)
    {
        var (user, userError) = GetUserByRefreshToken(token);

        if (user == null)
            return (false, userError);

        var refreshToken = user.RefreshTokens.Single(x => x.Token == token);

        if (!refreshToken.IsActive)
            return (false, "Invalid token");
        
        RevokeRefreshToken(refreshToken, ipAddress, "Revoked without replacement");

        db.Update(user);
        db.SaveChanges();

        return (true, null);
    }

    private (UserEntity? user, string? error) GetUserByRefreshToken(string token)
    {
        var user = db.Users.Include(u => u.RefreshTokens).SingleOrDefault(u => u.RefreshTokens.Any(t => t.Token == token));
        return (user, user == null ? "Invalid token" : null);
    }

    private RefreshTokenEntity ReplaceOldRefreshToken(UserEntity user, RefreshTokenEntity tokenToRevoke, string ipAddress)
    {
        var newRefreshToken = jwtService.GenerateRefreshToken(user, ipAddress);
        RevokeRefreshToken(tokenToRevoke, ipAddress, "Replaced by new token", newRefreshToken.Token);
        return newRefreshToken;
    }

    private void RemoveOldRefreshTokens(UserEntity userEntity)
    {
        var now = DateTime.UtcNow;

        foreach (var refreshToken in userEntity.RefreshTokens)
        {
            if (!refreshToken.IsActive &&
                refreshToken.Created.AddDays(jwtSettings.RefreshTokenTTLInDays) <= now)
            {
                db.RefreshTokens.Remove(refreshToken);
            }
        }
    }

    private void RevokeDescendantRefreshTokens(RefreshTokenEntity refreshTokenEntity, UserEntity userEntity, string ipAddress, string reason)
    {
        if (string.IsNullOrEmpty(refreshTokenEntity.ReplacedByToken))
            return;
        
        var childToken = userEntity.RefreshTokens.SingleOrDefault(x => x.Token == refreshTokenEntity.ReplacedByToken);

        if (childToken == null)
            return;

        if (childToken.IsActive)
            RevokeRefreshToken(childToken, ipAddress, reason);
        else
            RevokeDescendantRefreshTokens(childToken, userEntity, ipAddress, reason);
    }

    private static void RevokeRefreshToken(
        RefreshTokenEntity tokenEntity, 
        string ipAddress, 
        string reason = null, 
        string replacedByToken = null)
    {
        tokenEntity.Revoked = DateTime.UtcNow;
        tokenEntity.RevokedByIp = ipAddress;
        tokenEntity.ReasonRevoked = reason;
        tokenEntity.ReplacedByToken = replacedByToken;
    }
}
