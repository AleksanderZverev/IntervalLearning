using System.Globalization;
using DB;
using DB.Models;
using IntervalLearningApi.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace IntervalLearningApi.Controllers.Users;

public class AuthenticationService : IAuthenticationService
{
    private ApplicationContext _context;
    private IJwtUtils _jwtUtils;
    private readonly JwtSettings _jwtSettings;

    public AuthenticationService(
        ApplicationContext context,
        IJwtUtils jwtUtils,
        IOptions<JwtSettings> appSettings)
    {
        _context = context;
        _jwtUtils = jwtUtils;
        _jwtSettings = appSettings.Value;
    }

    public AuthenticateResponse Authenticate(AuthenticateRequest model, string ipAddress)
    {
        var user = _context.Users.Include(u => u.PasswordHash).SingleOrDefault(x => x.Email == model.Email);

        // validate
        if (user == null || !BCrypt.Net.BCrypt.Verify(model.Password, user.PasswordHash.PasswordHash))
            throw new AppException("Email or password is incorrect");

        // authentication successful so generate jwt and refresh tokens
        var jwtToken = _jwtUtils.GenerateJwtToken(user);
        var refreshToken = _jwtUtils.GenerateRefreshToken(ipAddress);
        user.RefreshTokens.Add(refreshToken);

        // remove old refresh tokens from userEntity
        removeOldRefreshTokens(user);

        // save changes to db
        _context.Update(user);
        _context.SaveChanges();

        return new AuthenticateResponse(user, jwtToken, refreshToken.Token);
    }

    public AuthenticateResponse RefreshToken(string token, string ipAddress)
    {
        var user = getUserByRefreshToken(token);
        var refreshToken = user.RefreshTokens.Single(x => x.Token == token);

        if (refreshToken.IsRevoked)
        {
            // revoke all descendant tokens in case this token has been compromised
            revokeDescendantRefreshTokens(refreshToken, user, ipAddress, $"Attempted reuse of revoked ancestor token: {token}");
            _context.Update(user);
            _context.SaveChanges();
        }

        if (!refreshToken.IsActive)
            throw new AppException("Invalid token");

        // replace old refresh token with a new one (rotate token)
        var newRefreshToken = rotateRefreshToken(refreshToken, ipAddress);
        user.RefreshTokens.Add(newRefreshToken);

        // remove old refresh tokens from userEntity
        removeOldRefreshTokens(user);

        // save changes to db
        _context.Update(user);
        _context.SaveChanges();

        // generate new jwt
        var jwtToken = _jwtUtils.GenerateJwtToken(user);

        return new AuthenticateResponse(user, jwtToken, newRefreshToken.Token);
    }

    public void RevokeToken(string token, string ipAddress)
    {
        var user = getUserByRefreshToken(token);
        var refreshToken = user.RefreshTokens.Single(x => x.Token == token);

        if (!refreshToken.IsActive)
            throw new AppException("Invalid token");

        // revoke token and save
        revokeRefreshToken(refreshToken, ipAddress, "Revoked without replacement");
        _context.Update(user);
        _context.SaveChanges();
    }

    public IEnumerable<UserEntity> GetAll()
    {
        return _context.Users;
    }

    public UserEntity GetById(int id)
    {
        var user = _context.Users.Find(id);
        if (user == null) throw new KeyNotFoundException("UserEntity not found");
        return user;
    }

    // helper methods

    private UserEntity getUserByRefreshToken(string token)
    {
        var user = _context.Users.SingleOrDefault(u => u.RefreshTokens.Any(t => t.Token == token));

        if (user == null)
            throw new AppException("Invalid token");

        return user;
    }

    private RefreshTokenEntity rotateRefreshToken(RefreshTokenEntity refreshTokenEntity, string ipAddress)
    {
        var newRefreshToken = _jwtUtils.GenerateRefreshToken(ipAddress);
        revokeRefreshToken(refreshTokenEntity, ipAddress, "Replaced by new token", newRefreshToken.Token);
        return newRefreshToken;
    }

    private void removeOldRefreshTokens(UserEntity userEntity)
    {
        // remove old inactive refresh tokens from userEntity based on TTL in app settings
        userEntity.RefreshTokens.RemoveAll(x =>
            !x.IsActive &&
            x.Created.AddDays(_jwtSettings.RefreshTokenTTLInDays) <= DateTime.UtcNow);
    }

    private void revokeDescendantRefreshTokens(RefreshTokenEntity refreshTokenEntity, UserEntity userEntity, string ipAddress, string reason)
    {
        if (string.IsNullOrEmpty(refreshTokenEntity.ReplacedByToken))
            return;

        // recursively traverse the refresh token chain and ensure all descendants are revoked
        var childToken = userEntity.RefreshTokens.SingleOrDefault(x => x.Token == refreshTokenEntity.ReplacedByToken);
        if (childToken.IsActive)
            revokeRefreshToken(childToken, ipAddress, reason);
        else
            revokeDescendantRefreshTokens(childToken, userEntity, ipAddress, reason);
    }

    private void revokeRefreshToken(RefreshTokenEntity tokenEntity, string ipAddress, string reason = null, string replacedByToken = null)
    {
        tokenEntity.Revoked = DateTime.UtcNow;
        tokenEntity.RevokedByIp = ipAddress;
        tokenEntity.ReasonRevoked = reason;
        tokenEntity.ReplacedByToken = replacedByToken;
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