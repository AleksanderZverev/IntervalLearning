using Domain.User;
using Domain.User.Entities;
using Domain.User.ValueObjects;
using DomainServices.DB.Queries.Accounts;
using DomainServices.DB.Repositories.Accounts.Users;

namespace DomainServices.DB.Repositories.Accounts;

public interface IAccountRepository : IBoundedContextRepository
{
    IAccountQueryRepository Query { get; }
    IRepository<User, UserId, UserIdParams> Users { get; }
    IRepository<UserPassword> Passwords { get; }
    IRepository<UserMetadata> Metadata { get; }
    IRepository<RefreshTokenEntity> RefreshTokens { get; }
}