using Application.Common.Interfaces.DB.Queries.Accounts.RefreshTokens;
using DB.Models;
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
}