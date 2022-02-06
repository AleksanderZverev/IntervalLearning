using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using IntervalLearningApi.Models;
using Microsoft.IdentityModel.Tokens;

namespace IntervalLearningApi.Helpers
{
    public static class JwtHelpers
    {
        public static IEnumerable<Claim> GetClaims(this UserTokens userAccounts, Guid id)
        {
            var claims = new Claim[]
            {
                new("Id", userAccounts.Id.ToString()),
                new(ClaimTypes.Name, userAccounts.UserName),
                new(ClaimTypes.Email, userAccounts.EmailId),
                new(ClaimTypes.NameIdentifier, id.ToString()),
                new(ClaimTypes.Expiration, DateTime.UtcNow.AddDays(1).ToString("MMM ddd dd yyyy HH:mm:ss tt")),
            };
            return claims;
        }

        public static IEnumerable<Claim> GetClaims(this UserTokens userAccounts, out Guid id)
        {
            id = Guid.NewGuid();
            return GetClaims(userAccounts, id);
        }

        public static UserTokens GenTokenKey(UserTokens model, JwtSettings jwtSettings)
        {
            try
            {
                var userToken = new UserTokens();

                if (model == null) 
                    throw new ArgumentException(nameof(model));

                var secretKey = System.Text.Encoding.ASCII.GetBytes(jwtSettings.IssuerSigningKey);
                var expireTime = DateTime.UtcNow.AddDays(1);
                userToken.Validaty = expireTime.TimeOfDay;
                var claims = GetClaims(model, out var userId);

                var jwtToken = new JwtSecurityToken(
                    issuer: jwtSettings.ValidIssuer, 
                    audience: jwtSettings.ValidAudience,
                    claims: claims, 
                    notBefore: new DateTimeOffset(DateTime.Now).DateTime,
                    expires: new DateTimeOffset(expireTime).DateTime,
                    signingCredentials: new SigningCredentials(
                        new SymmetricSecurityKey(secretKey),
                        SecurityAlgorithms.HmacSha256));

                var jwtTokenString = new JwtSecurityTokenHandler().WriteToken(jwtToken);

                userToken.Token = jwtTokenString;
                userToken.UserName = model.UserName;
                userToken.Id = model.Id;
                userToken.GuidId = userId;

                return userToken;
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
