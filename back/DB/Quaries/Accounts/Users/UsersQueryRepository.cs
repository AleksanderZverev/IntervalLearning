using Application.Common.Interfaces.DB.Queries.Accounts.Users;
using Domain.User;
using Domain.User.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace DB.Quaries.Accounts.Users;

public class UsersQueryRepository : IUsersQueryRepository
{
    private readonly ApplicationContext db;

    public UsersQueryRepository(ApplicationContext db)
    {
        this.db = db;
    }

    public Task<User?> FindByEmail(EmailAddress email)
    {
        return db.Users
            .Include(u => u.PasswordHash)
            .Include(u => u.RefreshTokens)
            .Include(u => u.Metadata)
            .SingleOrDefaultAsync(x => EF.Functions.ILike(x.Email, email.Value));
    }
}