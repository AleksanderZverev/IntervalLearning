using Application.Common.Interfaces.DB.Queries.Accounts;
using Application.Common.Interfaces.DB.Queries.Accounts.Users;
using Application.Common.Interfaces.DB.Repositories.Accounts.Users;
using DB.Models;
using Domain.User;
using Domain.User.Entities;
using Domain.User.ValueObjects;

namespace Application.Common.Interfaces.DB.Repositories.Accounts;

public interface IAccountRepository : IBoundedContextRepository
{
    IAccountQueryRepository Query { get; }
    IRepository<User, UserId, UserIdParams> Users { get; }
    IRepository<UserPassword> Passwords { get; }
    IRepository<UserMetadata> Metadata { get; }
}