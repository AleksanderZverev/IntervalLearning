using Domain.User;
using Domain.User.Entities;
using Domain.User.ValueObjects;
using DomainServices.DB.Queries.Accounts;
using DomainServices.DB.Repositories;
using DomainServices.DB.Repositories.Accounts;
using DomainServices.DB.Repositories.Accounts.Users;
using FluentResults;
using GlobalTools.Errors;

namespace DB.Repository.Accounts;

public class AccountRepository : IAccountRepository
{
    private readonly ApplicationContext db;
    public IAccountQueryRepository Query { get; }
    public IRepository<User, UserId, UserIdParams> Users { get; }
    public IRepository<UserPassword> Passwords { get; }
    public IRepository<UserMetadata> Metadata { get; }
    public IRepository<RefreshTokenEntity> RefreshTokens { get; }

    public AccountRepository(
        ApplicationContext db,
        IAccountQueryRepository query,
        IRepository<User, UserId, UserIdParams> users,
        IRepository<UserPassword> passwords, 
        IRepository<UserMetadata> metadata, 
        IRepository<RefreshTokenEntity> refreshTokens)
    {
        this.db = db;
        Query = query;
        Users = users;
        Passwords = passwords;
        Metadata = metadata;
        RefreshTokens = refreshTokens;
    }

    public Result SaveChanges()
    {
        return Result.OkIf(db.SoftSaveChanges(), new InternalError());
    }

    public async Task<Result> SaveChangesAsync()
    {
        return Result.OkIf(await db.SoftSaveChangesAsync(), new InternalError());
    }
}