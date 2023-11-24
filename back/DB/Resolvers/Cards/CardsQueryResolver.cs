using Application.Common.Interfaces.Domain.Cards;
using Domain.Card;
using Domain.Card.ValueObjects;
using Domain.Collection.ValueObjects;
using Domain.User.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace DB.Resolvers.Cards;

public class CardsQueryResolver : ICardsQueryResolver
{
    private readonly ApplicationContext db;

    public CardsQueryResolver(ApplicationContext db)
    {
        this.db = db;
    }
    
    public Task<Card?> Find(UserId userId, CollectionId collectionId, CardId cardId)
    {
        return db.Cards
            .Include(r => r.Remembers)
            .AsSplitQuery()
            .SingleOrDefaultAsync(c =>
                c.ParentUserId == userId && c.ParentCollectionId == collectionId && c.Id == cardId);
    }
    
    public Task<List<Card>> GetAll(UserId userId, CollectionId collectionId)
    {
        return db.Cards
            .Where(c => c.ParentUserId == userId && c.ParentCollectionId == collectionId)
            .ToListAsync();
    }

    public Task<List<Card>> GetRange(UserId userId, CollectionId collectionId, List<CardId> cardsIds)
    {
        return db.Cards
            .Where(c => c.ParentUserId == userId
                        && c.ParentCollectionId == collectionId
                        && cardsIds.Contains(c.Id))
            .ToListAsync();
    }
}