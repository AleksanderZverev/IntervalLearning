using DB;
using DB.Models;
using Microsoft.EntityFrameworkCore;
using NodaTime;

namespace IntervalLearningApi.Services;

public class CardsService
{
    private readonly ApplicationContext db;
    private readonly UserMetadataService metadataService;

    public CardsService(ApplicationContext db, UserMetadataService metadataService)
    {
        this.db = db;
        this.metadataService = metadataService;
    }

    public Task<List<CardEntity>> GetCards(long userId, short collectionId, int page, int count)
    {
        var toSkip = (page - 1) * count;

        return db.Cards
            .Where(c => c.ParentUserId == userId && c.ParentCollectionId == collectionId)
            .Skip(toSkip)
            .Take(count)
            .ToListAsync();
    }

    public async Task<(List<CardEntity>? cards, string? error)> GetNotStartedCards(long userId, short collectionId)
    {
        var collection = db.Collections
            .Include(c => c.DefaultRepeatsSchedule)
            .SingleOrDefault(c => c.ParentUserId == userId && c.Id == collectionId);

        if (collection == null)
            return (null, "Not found");

        var schedule = collection.DefaultRepeatsSchedule;

        if (schedule == null)
            throw new NotImplementedException();

        var cards = await db.Cards
            .Where(c => c.ParentUserId == userId && c.ParentCollectionId == collectionId && c.IsFinished == null)
            .Take(schedule.CardsCountPerPhase)
            .ToListAsync();

        return (cards, null);
    }

    public async Task<(List<CollectionEntity> collections, List<(Instant, CardEntity)> cards)> GetLearningCollectionWithCards(long userId)
    {
        var queueItems = db.Queue
            .Where(c => c.ParentUserId == userId)
            .ToList();

        if (queueItems.Count == 0)
        {
            return (new List<CollectionEntity>(0), new List<(Instant, CardEntity)>(0));
        }

        var collectionIds = new HashSet<short>();
        var collectionIdToCards = new Dictionary<short, HashSet<short>>();

        foreach (var queueItem in queueItems)
        {
            collectionIds.Add(queueItem.ParentCollectionId);
            if (!collectionIdToCards.ContainsKey(queueItem.ParentCollectionId))
            {
                collectionIdToCards.Add(queueItem.ParentCollectionId, new HashSet<short>());
            }

            collectionIdToCards[queueItem.ParentCollectionId].Add(queueItem.ParentCardId);
        }

        var collectionsResultTask = db.Collections
            .Where(c => c.ParentUserId == userId && collectionIds.Contains(c.Id))
            .ToListAsync();

        var cardsTasks = collectionIdToCards.Select(tuple =>
        {
            var collectionId = tuple.Key;
            var cards = tuple.Value;

            var cardsResultTask = db.Cards
                .Where(c => c.ParentUserId == userId && c.ParentCollectionId == collectionId && cards.Contains(c.Id))
                .ToListAsync();

            return cardsResultTask;
        }).ToList();

        var collectionsResult = await collectionsResultTask;
        var cards = (await Task.WhenAll(cardsTasks)).SelectMany(c => c).ToDictionary(c => $"{c.ParentCollectionId}-{c.Id}");

        var cardsWithDates = new List<(Instant, CardEntity)>(cards.Count);
        
        foreach (var queueItem in queueItems)
        {
            var card = cards[$"{queueItem.ParentCollectionId}-{queueItem.ParentCardId}"];
            cardsWithDates.Add((queueItem.Date, card));
        }

        return (collectionsResult, cardsWithDates);
    }

    public (CardEntity? card, string? error) Create(
        long userId,
        short collectionId,
        string frontText,
        string backText,
        long scheduleUserId,
        short scheduleId,
        string? description = null,
        List<string>? examples = null)
    {
        var card = new CardEntity(
            userId,
            collectionId,
            frontText,
            backText,
            scheduleUserId,
            scheduleId,
            description,
            examples
        );

        try
        {
            db.Entry(card).State = EntityState.Added;
            db.SaveChanges();
            return (card, null);
        }
        catch
        {
            return (null, "Unknown error");
        }
    }

    public (bool ok, string? reason) Start(long userId, short collectionId, short cardId) =>
        ChangeState(userId, collectionId, cardId, false);

    public (bool ok, string? reason) Finish(long userId, short collectionId, short cardId) =>
        ChangeState(userId, collectionId, cardId, true);

    public (bool ok, string? reason) SetNotStarted(long userId, short collectionId, short cardId) =>
        ChangeState(userId, collectionId, cardId, null);

    private (bool ok, string? reason) ChangeState(long userId, short collectionId, short cardId, bool? isFinished)
    {
        var cardEntity = db.Cards.Find(userId, collectionId, cardId);

        if (cardEntity == null)
            return (false, "Not found");

        if (cardEntity.IsFinished == isFinished)
            return (true, null);

        var metadata = metadataService.GetMetadata(userId);
        metadataService.CardStateChanged(metadata, cardEntity.IsFinished, isFinished);

        cardEntity.IsFinished = isFinished;
        db.SaveChanges();

        return (true, null);
    }

    public (bool ok, string? reason) Remember(
        long userId,
        short collectionId,
        short cardId,
        float weight,
        byte phaseStep,
        Instant repeatedDate)
    {
        var remembers = db.Remembers
            .Where(r => r.ParentUserId == userId &&
                        r.ParentCollectionId == collectionId &&
                        r.ParentCardId == cardId)
            .ToList();

        if (remembers.Any(r => r.PhaseStep >= phaseStep))
            return (false, "Conflict");

        db.Database.BeginTransaction();

        var remember = new RememberEntity(
            userId, collectionId, cardId, weight, phaseStep, repeatedDate);

        db.Remembers.Add(remember);
        db.SaveChanges();

        var queueItem = db.Queue.Single(q => q.ParentUserId == userId
                                             && q.ParentCollectionId == collectionId
                                             && q.ParentCardId == cardId
                                             && q.PhaseStep == phaseStep);

        db.Entry(queueItem).State = EntityState.Deleted;
        db.SaveChanges();

        db.Database.CommitTransaction();

        return (true, null);
    }
}