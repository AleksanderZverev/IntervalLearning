using Application.Common.Interfaces.DB.Queries.Accounts;
using Application.Common.Interfaces.DB.Queries.Accounts.Users;
using Application.Common.Interfaces.DB.Repositories;
using Application.Common.Interfaces.DB.Repositories.Accounts;
using Application.Common.Interfaces.DB.Repositories.Accounts.Users;
using DB.Models;
using Domain.User;
using Domain.User.Entities;
using Domain.User.ValueObjects;
using FluentResults;
using Infrastructure.Errors;

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