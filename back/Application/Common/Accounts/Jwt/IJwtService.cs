using DB.Models;
using Domain.User;
using Domain.User.ValueObjects;
using FluentResults;

namespace Application.Common.Accounts.Jwt;

public interface IJwtService
{
    public bool IsTokenExpired(DateTime refreshTokenCreatedDate);
    public string GenerateJwtToken(User userEntity);
    public Result<UserId> ValidateJwtToken(string token, DateTime? notValidTill = null);
    public RefreshTokenEntity GenerateRefreshToken(User user, string ipAddress);
}