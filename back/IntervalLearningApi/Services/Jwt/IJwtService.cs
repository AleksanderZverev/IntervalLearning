using DB.Models;
using Domain.User;

namespace IntervalLearningApi.Services.Jwt;

public interface IJwtService
{
    public string GenerateJwtToken(User userEntity);
    public long? ValidateJwtToken(string token, DateTime? notValidTill = null);
    public RefreshTokenEntity GenerateRefreshToken(User user, string ipAddress);
}