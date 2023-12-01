using DB.Models;
using Domain.User.ValueObjects;

namespace Application.Common.Interfaces.DB.Queries.Accounts.RefreshTokens;

public interface IRefreshTokensQueryRepository
{
    Task<bool> Contains(string token);
    Task<RefreshTokenEntity?> Find(UserId userId, string refreshToken);
}