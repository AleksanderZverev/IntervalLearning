using DB.Models;

namespace IntervalLearningApi.Controllers.Users;

public interface IJwtUtils
{
    public string GenerateJwtToken(UserEntity userEntity);
    public int? ValidateJwtToken(string token);
    public RefreshTokenEntity GenerateRefreshToken(string ipAddress);
}