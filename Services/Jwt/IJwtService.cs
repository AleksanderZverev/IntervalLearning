using DB.Models;

namespace IntervalLearningApi.Services.Jwt;

public interface IJwtService
{
    public string GenerateJwtToken(UserEntity userEntity);
    public int? ValidateJwtToken(string token);
    public RefreshTokenEntity GenerateRefreshToken(string ipAddress);
}