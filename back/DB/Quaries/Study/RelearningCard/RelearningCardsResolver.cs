using Application.Common.Interfaces.DB.Queries.Study.RelearningCards;
using Domain.Collection;
using Domain.Collection.ValueObjects;
using Domain.User.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace DB.Quaries.Study.RelearningCard;

public class RelearningCardsResolver : IRelearningCardsResolver
{
    private readonly ApplicationContext db;

    public RelearningCardsResolver(ApplicationContext db)
    {
        this.db = db;
    }

    public Task<List<Domain.RelearningCard.RelearningCard>> GetAll(UserId userId)
    {
        return db.RelearningCards
            .Where(c => c.UserId == userId)
            .ToListAsync();
    }

    public Task<List<Domain.RelearningCard.RelearningCard>> GetAllFor(UserId userId, CollectionId collectionId)
    {
        return db.RelearningCards
            .Where(c => c.UserId == userId && c.CollectionId == collectionId)
            .ToListAsync();
    }
}