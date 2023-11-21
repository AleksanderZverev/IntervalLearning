using DB;
using DB.BusinessExtensions;
using DB.Models;
using Domain.Language.ValueObjects;
using Domain.User;
using Domain.User.Entities;
using Domain.User.ValueObjects;
using FluentResults;
using Infrastructure.Errors;
using IntervalLearningApi.Models;
using IntervalLearningApi.Models.Common;
using IntervalLearningApi.Services.Jwt;
using Microsoft.EntityFrameworkCore;
using UserMetadata = DB.Models.UserMetadata;

namespace IntervalLearningApi.Services.Authentication;

public class AuthenticationService : IAuthenticationService
{
    private readonly IDateTimeProvider dateTimeProvider;
    private readonly ApplicationContext db;
    private readonly IJwtService jwtService;
    private readonly JwtSettings jwtSettings;

    public AuthenticationService(
        IDateTimeProvider dateTimeProvider,
        ApplicationContext db,
        IJwtService jwtService,
        JwtSettings jwtSettings)
    {
        this.dateTimeProvider = dateTimeProvider;
        this.db = db;
        this.jwtService = jwtService;
        this.jwtSettings = jwtSettings;
    }

    public Result Register(RegisterRequest request, string sourceIpAddress)
    {
        var emailLower = request.Email.ToLowerInvariant();
        var sameUser = db.Users.FirstOrDefault(u => u.Email == emailLower);

        if (sameUser != null)
            return new ConflictError("Email");

        var userIdResult = db.GetUniqueUserId();
        if (userIdResult.IsFailed)
            return new InternalError();
        
        //TODO: validation
        var user = User.Create(
            userIdResult.Value,
            EmailAddress.Create(emailLower).Value,
            UserName.Create(request.FirstName, request.LastName).Value
        ).Value;
        
        var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);
        user.PasswordHash = UserPassword.Create(userIdResult.Value, passwordHash).Value;
        
        try
        {
            db.Database.BeginTransaction();

            db.Users.Add(user);
            db.UsersPasswords.Add(user.PasswordHash);
            db.SaveChanges();

            //TODO: validation
            var metadata = new UserMetadata(user.Id, LanguageId.Create(request.SuggestLanguageId).Value);
            db.Entry(metadata).State = EntityState.Added;

            db.SaveChanges();
            db.Database.CommitTransaction();
        }
        catch
        {
            return new InternalError();
        }

        return Result.Ok();
    }

    public Result<AuthenticateResponse> Authenticate(AuthenticateRequest req, string ipAddress)
    {
        var user = db.Users
            .Include(u => u.PasswordHash)
            .Include(u => u.RefreshTokens)
            .Include(u => u.Metadata)
            .SingleOrDefault(x => x.Email == req.Email.ToLower());

        if (user is {PasswordHash: null})
            return new BadRequestError("User is not signed up");

        if (user == null || !BCrypt.Net.BCrypt.Verify(req.Password, user.PasswordHash.PasswordHash))
            return new BadRequestError("Email or password is incorrect");

        return Authenticate(user, ipAddress);
    }

    public AuthenticateResponse? TryAuthenticateByOldToken(string jwtToken, string refreshToken)
    {
        var userId = jwtService.ValidateJwtToken(jwtToken, dateTimeProvider.UtcNow.AddMinutes(5));

        if (userId == null)
            return null;

        var user = db.Users
            .Include(u => u.Metadata)
            .Single(u => u.Id == userId);
        
        return ToAuthenticationResponse(user, jwtToken, refreshToken);
    }

    private AuthenticateResponse Authenticate(User user, string ipAddress)
    {
        var jwtToken = jwtService.GenerateJwtToken(user);
        var refreshToken = jwtService.GenerateRefreshToken(user, ipAddress);

        user.RefreshTokens.Add(refreshToken);

        RemoveOldRefreshTokens(user);
        db.SaveChanges();

        return ToAuthenticationResponse(user, jwtToken, refreshToken.Token);
    }

    public Result<AuthenticateResponse> RefreshToken(string refreshToken, string ipAddress)
    {
        var userResult = GetUserByRefreshToken(refreshToken);

        if (userResult.IsFailed)
            return userResult.ToResult();

        var user = userResult.Value;
        var refreshTokenItem = user.RefreshTokens.Single(x => x.Token == refreshToken);

        if (refreshTokenItem.IsRevoked)
        {
            RevokeDescendantRefreshTokens(refreshTokenItem, user, ipAddress, $"Attempted reuse of revoked ancestor token: {refreshToken}");
            db.Update(user);
            db.SaveChanges();
        }

        if (!refreshTokenItem.IsActive)
            return new BadRequestError("Refresh token is invalid");

        var newRefreshToken = ReplaceOldRefreshToken(user, refreshTokenItem, ipAddress);
        user.RefreshTokens.Add(newRefreshToken);
        
        RemoveOldRefreshTokens(user);

        if (!db.SoftSaveChanges())
        {
            return new InternalError();
        }

        var jwtToken = jwtService.GenerateJwtToken(user);
        return ToAuthenticationResponse(user, jwtToken, newRefreshToken.Token);
    }

    private AuthenticateResponse ToAuthenticationResponse(User userEntity, string jwtToken, string refreshToken)
    {
        return new AuthenticateResponse()
        {
            Id = userEntity.Id.ToString(),
            FirstName = userEntity.UserName.FirstName,
            LastName = userEntity.UserName.LastName,
            Email = userEntity.Email,
            JwtToken = jwtToken,
            RefreshToken = refreshToken,
            SuggestTranslationLanguageId = userEntity.Metadata.SuggestTranslationLanguageId.ToString(),
        };
    }

    public Result RevokeToken(string token, string ipAddress)
    {
        var userResult = GetUserByRefreshToken(token);

        if (userResult.IsFailed)
            return userResult.ToResult();

        var user = userResult.Value;
        var refreshToken = user.RefreshTokens.Single(x => x.Token == token);

        if (!refreshToken.IsActive)
            return new BadRequestError("Refresh token is invalid");
        
        RevokeRefreshToken(refreshToken, ipAddress, "Revoked without replacement");

        db.Update(user);
        return db.SoftSaveChanges()
            ? Result.Ok()
            : new InternalError();
    }

    private Result<User> GetUserByRefreshToken(string token)
    {
        var user = db.Users
            .Include(u => u.RefreshTokens)
            .Include(u => u.Metadata)
            .SingleOrDefault(u => u.RefreshTokens.Any(t => t.Token == token));
        
        return user != null ? user : new BadRequestError("Refresh token is expired");
    }

    private RefreshTokenEntity ReplaceOldRefreshToken(User user, RefreshTokenEntity tokenToRevoke, string ipAddress)
    {
        var newRefreshToken = jwtService.GenerateRefreshToken(user, ipAddress);
        RevokeRefreshToken(tokenToRevoke, ipAddress, "Replaced by new token", newRefreshToken.Token);
        return newRefreshToken;
    }

    private void RemoveOldRefreshTokens(User userEntity)
    {
        var now = dateTimeProvider.UtcNow;

        foreach (var refreshToken in userEntity.RefreshTokens)
        {
            if (!refreshToken.IsActive &&
                refreshToken.Created.AddDays(jwtSettings.RefreshTokenTTLInDays) <= now)
            {
                db.RefreshTokens.Remove(refreshToken);
            }
        }
    }

    private void RevokeDescendantRefreshTokens(RefreshTokenEntity refreshTokenEntity, User userEntity, string ipAddress, string reason)
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

    private void RevokeRefreshToken(
        RefreshTokenEntity tokenEntity, 
        string ipAddress, 
        string reason = null, 
        string replacedByToken = null)
    {
        tokenEntity.Revoked = dateTimeProvider.UtcNow;
        tokenEntity.RevokedByIp = ipAddress;
        tokenEntity.ReasonRevoked = reason;
        tokenEntity.ReplacedByToken = replacedByToken;
    }
}
