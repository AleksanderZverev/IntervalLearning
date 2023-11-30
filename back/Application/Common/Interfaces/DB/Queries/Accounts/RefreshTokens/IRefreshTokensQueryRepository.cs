using DB.Models;

namespace Application.Common.Interfaces.DB.Queries.Accounts.RefreshTokens;

public interface IRefreshTokensQueryRepository
{
    Task<bool> Contains(string token);
}