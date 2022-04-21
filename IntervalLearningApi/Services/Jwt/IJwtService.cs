using DB.Models;

namespace IntervalLearningApi.Services.Jwt;

public interface IJwtService
{
    public string GenerateJwtToken(UserEntity userEntity);
    public long? ValidateJwtToken(string token, DateTime? notValidTill = null);
    public RefreshTokenEntity GenerateRefreshToken(UserEntity user, string ipAddress);
}