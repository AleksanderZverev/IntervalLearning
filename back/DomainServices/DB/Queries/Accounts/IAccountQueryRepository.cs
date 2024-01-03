using DomainServices.DB.Queries.Accounts.RefreshTokens;
using DomainServices.DB.Queries.Accounts.Users;
using DomainServices.DB.Repositories;

namespace DomainServices.DB.Queries.Accounts;

public interface IAccountQueryRepository : IBoundedContextQueryRepository
{
    IUsersQueryRepository Users { get; }
    IRefreshTokensQueryRepository RefreshTokens { get; }
}