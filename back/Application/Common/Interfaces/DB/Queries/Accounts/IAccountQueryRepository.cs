using Application.Common.Interfaces.DB.Queries.Accounts.Users;
using Application.Common.Interfaces.DB.Repositories;

namespace Application.Common.Interfaces.DB.Queries.Accounts;

public interface IAccountQueryRepository : IBoundedContextRepository
{
    IUsersQueryRepository Users { get; }
}