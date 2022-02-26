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
    private readonly JwtSettings jwtSettings;

    public AuthenticationService(
        ApplicationContext db,
        IJwtService jwtService,
        IOptions<JwtSettings> jwtSettings)
    {
        this.db = db;
        this.jwtService = jwtService;
        this.jwtSettings = jwtSettings.Value;
    }

    public AuthenticateResponse Authenticate(AuthenticateRequest req, string ipAddress)
    {
        var user = db.Users.Include(u => u.PasswordHash).SingleOrDefault(x => x.Email == req.Email);

        if (user is {PasswordHash: null})
            throw new AppException("Not signed up user!");

        if (user == null || !BCrypt.Net.BCrypt.Verify(req.Password, user.PasswordHash.PasswordHash))
            throw new AppException("Email or password is incorrect");

        var jwtToken = jwtService.GenerateJwtToken(user);
        var refreshToken = jwtService.GenerateRefreshToken(ipAddress);
        user.RefreshTokens.Add(refreshToken);
        
        RemoveOldRefreshTokens(user);
        
        db.Update(user);
        db.SaveChanges();

        return new AuthenticateResponse(user, jwtToken, refreshToken.Token);
    }

    public AuthenticateResponse RefreshToken(string refreshToken, string ipAddress)
    {
        var user = GetUserByRefreshToken(refreshToken);
        var refreshTokenItem = user.RefreshTokens.Single(x => x.Token == refreshToken);

        if (refreshTokenItem.IsRevoked)
        {
            RevokeDescendantRefreshTokens(refreshTokenItem, user, ipAddress, $"Attempted reuse of revoked ancestor token: {refreshToken}");
            db.Update(user);
            db.SaveChanges();
        }

        if (!refreshTokenItem.IsActive)
            throw new AppException("Invalid token");
        
        var newRefreshToken = ReplaceOldRefreshToken(refreshTokenItem, ipAddress);
        user.RefreshTokens.Add(newRefreshToken);
        
        RemoveOldRefreshTokens(user);
        
        db.Update(user);
        db.SaveChanges();
        
        var jwtToken = jwtService.GenerateJwtToken(user);
        return new AuthenticateResponse(user, jwtToken, newRefreshToken.Token);
    }

    public void RevokeToken(string token, string ipAddress)
    {
        var user = GetUserByRefreshToken(token);
        var refreshToken = user.RefreshTokens.Single(x => x.Token == token);

        if (!refreshToken.IsActive)
            throw new AppException("Invalid token");
        
        RevokeRefreshToken(refreshToken, ipAddress, "Revoked without replacement");

        db.Update(user);
        db.SaveChanges();
    }

    private UserEntity GetUserByRefreshToken(string token)
    {
        var user = db.Users.SingleOrDefault(u => u.RefreshTokens.Any(t => t.Token == token));

        if (user == null)
            throw new AppException("Invalid token");

        return user;
    }

    private RefreshTokenEntity ReplaceOldRefreshToken(RefreshTokenEntity refreshToken, string ipAddress)
    {
        var newRefreshToken = jwtService.GenerateRefreshToken(ipAddress);
        RevokeRefreshToken(refreshToken, ipAddress, "Replaced by new token", newRefreshToken.Token);
        return newRefreshToken;
    }

    private void RemoveOldRefreshTokens(UserEntity userEntity)
    {
        userEntity.RefreshTokens.RemoveAll(x =>
            !x.IsActive &&
            x.Created.AddDays(jwtSettings.RefreshTokenTTLInDays) <= DateTime.UtcNow);
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

public class UserService
{
    private readonly ApplicationContext db;
    private readonly IJwtService jwtService;
    private readonly JwtSettings _jwtSettings;

    public UserService(
        ApplicationContext db,
        IJwtService jwtService,
        IOptions<JwtSettings> appSettings)
    {
        this.db = db;
        this.jwtService = jwtService;
        _jwtSettings = appSettings.Value;
    }

    public IEnumerable<UserEntity> GetAll()
    {
        return db.Users;
    }

    public UserEntity GetById(int id)
    {
        var user = db.Users.Find(id);
        if (user == null) throw new KeyNotFoundException("UserEntity not found");
        return user;
    }
}

public class AppException : Exception
{
    public AppException() : base() { }

    public AppException(string message) : base(message) { }

    public AppException(string message, params object[] args)
        : base(String.Format(CultureInfo.CurrentCulture, message, args))
    {
    }
}