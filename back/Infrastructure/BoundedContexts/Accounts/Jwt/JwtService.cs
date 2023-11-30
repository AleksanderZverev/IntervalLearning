using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Application.Common.Accounts.Jwt;
using Application.Common.Interfaces.DB.Queries.Accounts;
using DB.Models;
using Domain.User;
using Domain.User.ValueObjects;
using FluentResults;
using Infrastructure;
using Infrastructure.BoundedContexts.Accounts.Jwt;
using Infrastructure.Errors;
using Microsoft.IdentityModel.Tokens;

namespace IntervalLearningApi.Services.Jwt;

public class JwtService : IJwtService
{
    private const string IdClaimType = "Id";
    private readonly IDateTimeProvider dateTimeProvider;
    private readonly IAccountQueryRepository accountQueryRepository;
    private readonly JwtSettings jwtSettings;

    public JwtService(
        IDateTimeProvider dateTimeProvider,
        IAccountQueryRepository accountQueryRepository,
        JwtSettings appSettings)
    {
        this.dateTimeProvider = dateTimeProvider;
        this.accountQueryRepository = accountQueryRepository;
        jwtSettings = appSettings;
    }

    public bool IsTokenExpired(DateTime refreshTokenCreatedDate)
    {
        return refreshTokenCreatedDate.AddDays(jwtSettings.RefreshTokenTTLInDays) <= dateTimeProvider.UtcNow;
    }

    public string GenerateJwtToken(User userEntity)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var securityKey = Encoding.ASCII.GetBytes(jwtSettings.Secret);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(GetClaims(userEntity)),
            Expires = dateTimeProvider.UtcNow.AddMinutes(jwtSettings.JwtTokenTTLInMinutes),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(securityKey), SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    public Result<UserId> ValidateJwtToken(string token, DateTime? notValidTill = null)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var securityKey = Encoding.ASCII.GetBytes(jwtSettings.Secret);

        try
        {
            tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(securityKey),
                ValidateIssuer = false,
                ValidateAudience = false,
                ClockSkew = TimeSpan.Zero,  // if zero then tokens expire exactly at token expiration time (instead of 5 minutes later)
            }, out var validatedToken);

            var jwtToken = (JwtSecurityToken)validatedToken;

            var expireTime = jwtToken.IssuedAt.AddMinutes(jwtSettings.JwtTokenTTLInMinutes);

            if (notValidTill != null && expireTime <= notValidTill)
            {
                return new BadRequestError("Token is not valid");
            }

            var userId = long.Parse(jwtToken.Claims.First(x => x.Type == IdClaimType).Value);

            return UserId.Create(userId);
        }
        catch
        {
            return new InternalError();
        }
    }

    public RefreshTokenEntity GenerateRefreshToken(User user, string ipAddress)
    {
        var now = dateTimeProvider.UtcNow; // SystemClock.Instance.GetCurrentInstant();

        var lastRefreshToken = user.RefreshTokens.MaxBy(t => t.Id);
        var id = (short) (lastRefreshToken == null ? 0 : (lastRefreshToken.Id + 1) % short.MaxValue);

        var refreshToken = new RefreshTokenEntity
        {
            Id = id,
            Token = GetUniqueToken(),
            Expires = now + TimeSpan.FromDays(jwtSettings.RefreshTokenTTLInDays), //Duration.FromDays(jwtSettings.RefreshTokenTTLInDays),
            Created = now,
            CreatedByIp = ipAddress,
            ParentUserId = user.Id,
        };

        return refreshToken;

        string GetUniqueToken()
        {
            var randomSequence = RandomNumberGenerator.GetBytes(64);
            var token = Convert.ToBase64String(randomSequence);
            var isTokenUnique = !accountQueryRepository.RefreshTokens.Contains(token).GetAwaiter().GetResult();
            return isTokenUnique ? token : GetUniqueToken();
        }
    }

    public static IEnumerable<Claim> GetClaims(User user)
    {
        var claims = new Claim[]
        {
            new(IdClaimType, user.Id.ToString()),

            //For User.Identity
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),

            //For User.Identity.Name
            new(ClaimTypes.Name, user.Email),

            //new(ClaimTypes.Email, user.Email),
        };
        return claims;
    }
}