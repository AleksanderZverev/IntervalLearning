using Domain.User.Entities;
using Domain.User.ValueObjects;

namespace DomainServices.DB.Queries.Accounts.RefreshTokens;

public interface IRefreshTokensQueryRepository
{
    Task<bool> Contains(string token);
    Task<RefreshTokenEntity?> Find(UserId userId, string refreshToken);
}