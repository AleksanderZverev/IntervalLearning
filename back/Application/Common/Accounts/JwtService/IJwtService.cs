using Domain.User;
using Domain.User.Entities;
using Domain.User.ValueObjects;
using FluentResults;

namespace Application.Common.Accounts.JwtService;

public interface IJwtService
{
    public bool IsTokenExpired(DateTime refreshTokenCreatedDate);
    public string GenerateJwtToken(User user);
    public Result<UserId> ValidateJwtToken(string token, DateTime? notValidTill = null);
    public RefreshTokenEntity GenerateRefreshToken(User user, string ipAddress);
}