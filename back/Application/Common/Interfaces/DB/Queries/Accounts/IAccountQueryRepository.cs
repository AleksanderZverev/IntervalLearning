using Application.Common.Interfaces.DB.Queries.Accounts.RefreshTokens;
using Application.Common.Interfaces.DB.Queries.Accounts.Users;
using Application.Common.Interfaces.DB.Repositories;

namespace Application.Common.Interfaces.DB.Queries.Accounts;

public interface IAccountQueryRepository : IBoundedContextQueryRepository
{
    IUsersQueryRepository Users { get; }
    IRefreshTokensQueryRepository RefreshTokens { get; }
}