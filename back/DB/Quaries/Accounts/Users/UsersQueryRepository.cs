using Domain.User;
using Domain.User.ValueObjects;
using DomainServices.DB.Queries.Accounts.Users;
using Microsoft.EntityFrameworkCore;

namespace DB.Quaries.Accounts.Users;

public class UsersQueryRepository : IUsersQueryRepository
{
    private readonly ApplicationContext db;

    public UsersQueryRepository(ApplicationContext db)
    {
        this.db = db;
    }

    private IQueryable<User> Query => db.Users
        .Include(u => u.PasswordHash)
        .Include(u => u.RefreshTokens)
        .Include(u => u.Metadata)
        .AsSplitQuery();

    public Task<User?> FindByEmail(EmailAddress email)
    {
        return Query.SingleOrDefaultAsync(x => EF.Functions.ILike(x.Email, email.Value));
    }

    public Task<User?> Find(UserId userId)
    {
        return Query.SingleOrDefaultAsync(u => u.Id == userId);
    }

    public Task<User?> FindUserByRefreshToken(string refreshToken)
    {
        return Query.SingleOrDefaultAsync(u => u.RefreshTokens.Any(t => t.Token == refreshToken));
    }
}