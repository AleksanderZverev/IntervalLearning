using DB.Models;
using Domain.User;

namespace Application.Common.Accounts.Jwt;

public interface IJwtService
{
    public bool IsTokenExpired(DateTime refreshTokenCreatedDate);
    public string GenerateJwtToken(User userEntity);
    public long? ValidateJwtToken(string token, DateTime? notValidTill = null);
    public RefreshTokenEntity GenerateRefreshToken(User user, string ipAddress);
}