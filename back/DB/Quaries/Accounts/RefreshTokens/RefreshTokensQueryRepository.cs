using Application.Common.Interfaces.DB.Queries.Accounts.RefreshTokens;
using DB.Models;
using Domain.User.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace DB.Quaries.Accounts.RefreshTokens;

public class RefreshTokensQueryRepository : IRefreshTokensQueryRepository
{
    private readonly ApplicationContext db;

    public RefreshTokensQueryRepository(ApplicationContext db)
    {
        this.db = db;
    }

    public Task<bool> Contains(string token)
    {
        return db.RefreshTokens.AnyAsync(t => t.Token == token);
    }

    public Task<RefreshTokenEntity?> Find(UserId userId, string refreshToken)
    {
        return db.RefreshTokens.SingleOrDefaultAsync(t => t.ParentUserId == userId && t.Token == refreshToken);
    }
}