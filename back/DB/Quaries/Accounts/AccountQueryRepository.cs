using Application.Common.Interfaces.DB.Queries.Accounts;
using Application.Common.Interfaces.DB.Queries.Accounts.Users;

namespace DB.Quaries.Accounts;

public class AccountQueryRepository : IAccountQueryRepository
{
    public IUsersQueryRepository Users { get; }

    public AccountQueryRepository(
        IUsersQueryRepository users)
    {
        Users = users;
    }
}