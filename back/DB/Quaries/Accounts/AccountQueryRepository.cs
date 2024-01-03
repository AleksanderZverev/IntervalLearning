using DomainServices.DB.Queries.Accounts;
using DomainServices.DB.Queries.Accounts.RefreshTokens;
using DomainServices.DB.Queries.Accounts.Users;

namespace DB.Quaries.Accounts;

public class AccountQueryRepository : IAccountQueryRepository
{
    public IUsersQueryRepository Users { get; }
    public IRefreshTokensQueryRepository RefreshTokens { get; }

    public AccountQueryRepository(
        IUsersQueryRepository users, 
        IRefreshTokensQueryRepository refreshTokens)
    {
        Users = users;
        RefreshTokens = refreshTokens;
    }
}