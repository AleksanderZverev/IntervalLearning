using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using DB;
using DB.Models;
using IntervalLearningApi.Models;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using NodaTime;

namespace IntervalLearningApi.Services.Jwt;

public class JwtService : IJwtService
{
    private readonly ApplicationContext db;
    private readonly JwtSettings jwtSettings;

    public JwtService(
        ApplicationContext db,
        IOptions<JwtSettings> appSettings)
    {
        this.db = db;
        jwtSettings = appSettings.Value;
    }

    public string GenerateJwtToken(UserEntity userEntity)
    {
        const int tokenTtlInMinutes = 15;

        var tokenHandler = new JwtSecurityTokenHandler();
        var securityKey = Encoding.ASCII.GetBytes(jwtSettings.Secret);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(GetClaims(userEntity)),
            Expires = DateTime.UtcNow.AddMinutes(tokenTtlInMinutes),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(securityKey), SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    public int? ValidateJwtToken(string token)
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
            var userId = int.Parse(jwtToken.Claims.First(x => x.Type == "id").Value);
            
            return userId;
        }
        catch
        {
            return null;
        }
    }

    public RefreshTokenEntity GenerateRefreshToken(UserEntity user, string ipAddress)
    {
        var now = SystemClock.Instance.GetCurrentInstant();

        var lastRefreshToken = user.RefreshTokens.MaxBy(t => t.Id);
        var id = (byte)((lastRefreshToken.Id + 1) % byte.MaxValue);

        var refreshToken = new RefreshTokenEntity
        {
            Id = id,
            Token = GetUniqueToken(),
            Expires = now + Duration.FromDays(jwtSettings.RefreshTokenTTLInDays),
            Created = now,
            CreatedByIp = ipAddress,
            ParentUserId = user.Id,
        };

        return refreshToken;

        string GetUniqueToken()
        {
            var randomSequence = RandomNumberGenerator.GetBytes(64);
            var token = Convert.ToBase64String(randomSequence);

            var isTokenUnique = !db.Users.Any(u => u.RefreshTokens.Any(t => t.Token == token));

            return isTokenUnique ? token : GetUniqueToken();
        }
    }

    public static IEnumerable<Claim> GetClaims(UserEntity user)
    {
        var claims = new Claim[]
        {
            new("Id", user.Id.ToString()),

            //For User.Identity
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),

            //For User.Identity.Name
            new(ClaimTypes.Name, user.Email),

            //new(ClaimTypes.Email, user.Email),
        };
        return claims;
    }
}