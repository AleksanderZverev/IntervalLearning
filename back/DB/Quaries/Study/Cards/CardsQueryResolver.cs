using System.Linq.Expressions;
using Domain.Card;
using Domain.Card.ValueObjects;
using Domain.Collection.ValueObjects;
using Domain.User.ValueObjects;
using DomainServices.DB.Queries.Study.Cards;
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
            .Include(c => c.Remembers)
            .AsSplitQuery()
            .ToListAsync();
    }

    public Task<List<Card>> GetExceptRange(UserId userId, CollectionId collectionId, List<CardId> excludeCardIds)
    {
        return db.Cards
            .Where(c => c.ParentUserId == userId
                        && c.ParentCollectionId == collectionId
                        && !excludeCardIds.Contains(c.Id))
            .ToListAsync();
    }

    public async Task<List<Card>> Search(
        UserId userId,
        CollectionId collectionId,
        string searchValue,
        SearchFieldType fieldType,
        int page,
        int count)
    {
        Expression<Func<Card, bool>> condition = fieldType switch
        {
            SearchFieldType.RememberingText => c =>
                c.ParentUserId == userId
                && c.ParentCollectionId == collectionId
                && EF.Functions.ILike(c.RememberingText, $"{searchValue}%"),
            SearchFieldType.PromptText => c =>
                c.ParentUserId == userId
                && c.ParentCollectionId == collectionId
                && EF.Functions.ILike(c.PromptText, $"{searchValue}%"),
            SearchFieldType.MeaningText => c =>
                c.ParentUserId == userId
                && c.ParentCollectionId == collectionId
                && EF.Functions.ILike(c.MeaningText, $"{searchValue}%"),
            _ => throw new ArgumentOutOfRangeException(nameof(fieldType), fieldType, null)
        };

        var skip = (page - 1) * count;

        return await db.Cards
            .Where(condition)
            .OrderByDescending(c => c.CreatedDate)
            .Skip(skip)
            .Take(count)
            .ToListAsync();
    }

    public Task<List<Card>> GetRangeFromCollections(UserId userId, List<CollectionId> collectionIds)
    {
        return db.Cards
            .Where(c => c.ParentUserId == userId && collectionIds.Contains(c.ParentCollectionId))
            .ToListAsync();
    }

    public Task<bool> ContainsAny(UserId userId, CollectionId collectionId)
    {
        return db.Cards.AnyAsync(c => c.ParentUserId == userId && c.ParentCollectionId == collectionId);
    }

    public Task<int> CountByDateRange(UserId userId, CollectionId collectionId, DateTime from, DateTime to)
    {
        return db.Cards
            .Where(c => c.ParentUserId == userId
                        && c.ParentCollectionId == collectionId
                        && c.CreatedDate >= from
                        && c.CreatedDate <= to)
            .CountAsync();
    }

    public Task<int> CountStartedLearning(UserId userId, CollectionId collectionId)
    {
        return db.Cards
            .Where(c => c.ParentUserId == userId
                        && c.ParentCollectionId == collectionId
                        && c.Remembers.Count > 0)
            .CountAsync();
    }
}