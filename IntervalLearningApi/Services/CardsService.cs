using System.Diagnostics;
using DB;
using DB.Models;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.EntityFrameworkCore;

namespace IntervalLearningApi.Services;

public class CardsService
{
    private readonly ILogger<CardsService> logger;
    private readonly IWebHostEnvironment env;
    private readonly ApplicationContext db;
    private readonly UserMetadataService metadataService;

    public CardsService(ILogger<CardsService> logger,
        IWebHostEnvironment env,
        ApplicationContext db,
        UserMetadataService metadataService)
    {
        this.logger = logger;
        this.env = env;
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

    public async Task<List<CardEntity>> GetCardsQueue(long userId, short collectionId, DateTime date)
    {
        var queueItems = await db.Queue
            .Where(c => c.ParentUserId == userId 
                        && c.ParentCollectionId == collectionId 
                        && c.Date.Date == date.Date)
            .ToListAsync();


        if (queueItems.Count == 0)
            return new List<CardEntity>(0);

        var cardsIds = queueItems.Select(q => q.ParentCardId).ToList();

        var cards = await db.Cards
            .Where(c => c.ParentUserId == userId
                        && c.ParentCollectionId == collectionId
                        && cardsIds.Contains(c.Id))
            .ToListAsync();

        return cards;
    }

    public async Task<(List<CollectionEntity> collections, List<(DateTime, CardEntity)> cards)> GetLearningCollectionWithCards(long userId)
    {
        var queueItems = db.Queue
            .Where(c => c.ParentUserId == userId)
            .ToList();

        if (queueItems.Count == 0)
        {
            return (new List<CollectionEntity>(0), new List<(DateTime, CardEntity)>(0));
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

        var cardsWithDates = new List<(DateTime, CardEntity)>(cards.Count);
        
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

    public (DateTime? nextRepeatDate, string? reason) Start(long userId, short collectionId, List<short> cardIds)
    {
        db.Database.BeginTransaction();
        var (cards, error) = ChangeStates(userId, collectionId, cardIds, false);

        if (!string.IsNullOrEmpty(error))
        {
            db.Database.RollbackTransaction();
            return (null, error);
        }

        var (nextRepeatDate, queueError) =  AddToQueue(userId, collectionId, cards);

        if (!string.IsNullOrEmpty(queueError))
        {
            db.Database.RollbackTransaction();
            return (null, queueError);
        }

        db.Database.CommitTransaction();

        return (nextRepeatDate, null);
    }

    public (bool ok, string? reason) Start(long userId, short collectionId, short cardId) =>
        ChangeState(userId, collectionId, cardId, false);

    public (bool ok, string? reason) Finish(long userId, short collectionId, short cardId) =>
        ChangeState(userId, collectionId, cardId, true);

    public (bool ok, string? reason) SetNotStarted(long userId, short collectionId, short cardId) =>
        ChangeState(userId, collectionId, cardId, null);

    private (bool ok, string? reason) ChangeState(long userId, short collectionId, short cardId, bool? isFinished)
    {
        var collection = db.Collections.Find(userId, collectionId);

        if (collection == null)
            return (false, "card's collection not found");

        var cardEntity = db.Cards.Find(userId, collectionId, cardId);

        if (cardEntity == null)
            return (false, "Not found");

        if (cardEntity.IsFinished == isFinished)
            return (true, null);

        var metadata = metadataService.GetMetadata(userId);
        metadataService.CardStateChanged(metadata, collection, cardEntity.IsFinished, isFinished);

        cardEntity.IsFinished = isFinished;
        db.SaveChanges();

        return (true, null);
    }

    private (List<CardEntity> cards, string? reason) ChangeStates(long userId, short collectionId, List<short> cardIds, bool? isFinished)
    {
        var collection = db.Collections.Find(userId, collectionId);

        if (collection == null)
            return (new List<CardEntity>(0), "card's collection not found");

        var cards = db.Cards
            .Where(c => c.ParentUserId == userId
                        && c.ParentCollectionId == collectionId
                        && cardIds.Contains(c.Id))
            .Include(c => c.Remembers)
            .Include(c => c.ParentRepeatsSchedule).ThenInclude(c => c.Phases)
            .AsSplitQuery()
            .ToList();

        if (cards.Count == 0)
            return (new List<CardEntity>(0), "Not found");

        if (cards.Count != cardIds.Count)
        {
            logger.LogWarning("ChangeState: не все карты были найдены");
        }

        var metadata = metadataService.GetMetadata(userId);

        foreach (var cardEntity in cards)
        {
            if (cardEntity.IsFinished == isFinished)
            {
                logger.LogWarning("ChangeState: карта уже имеет такое состояние");
                throw new InvalidOperationException();
                //continue;
            }

            metadataService.CardStateChanged(metadata, collection, cardEntity.IsFinished, isFinished);
            cardEntity.IsFinished = isFinished;
            db.Attach(cardEntity).Property(c => c.IsFinished).IsModified = true;
        }

        db.Entry(collection).State = EntityState.Modified;
        db.Entry(metadata).State = EntityState.Modified;
        db.SaveChanges();

        return (cards, null);
    }

    private (DateTime? closestRepeatDate, string? reason) AddToQueue(long userId, short collectionId, List<CardEntity> cards)
    {
        var closestRepeatDate = DateTime.MaxValue;
        var queueItems = new List<CardRepeatQueueEntity>(cards.Count);

        foreach (var card in cards)
        {
            var lastRemember = card.Remembers.MaxBy(c => c.Id);
            var nextPhaseIndex = lastRemember?.PhaseStep ?? 0;
            var schedule = card.ParentRepeatsSchedule;

            if (schedule.Phases.Count == 0 || nextPhaseIndex >= schedule.Phases.Count)
            {
                Debug.Fail("schedule.Phases.Count == 0 ||  schedule.Phases.Count >= nextPhaseIndex");
                return (null, "Ошибка в работе алгоритма");
            }
                

            var nextPhase = schedule.Phases[nextPhaseIndex];
            var nextRepeatDate = DateTime.UtcNow.AddSeconds(nextPhase.SecondsFromLastPhase);

            if (nextRepeatDate <= closestRepeatDate)
                closestRepeatDate = nextRepeatDate;

            var queueItem = new CardRepeatQueueEntity(
                userId,
                collectionId,
                card.Id,
                (short)(nextPhaseIndex + 1),
                nextRepeatDate
            );

            queueItems.Add(queueItem);
        }

        queueItems.ForEach(q => db.Entry(q).State = EntityState.Added);
        db.SaveChanges();

#if DEBUG
        if (closestRepeatDate == DateTime.MaxValue)
            Debug.Fail("closestRepeatDate == DateTime.MaxValue)");
#endif

        return (closestRepeatDate, null);
    }

    public async Task<(bool ok, string? reason, DateTime? closestRepeatDate)> Remember(
        long userId,
        short collectionId,
        List<RememberItem> rememberItems, 
        DateTime date)
    {
        var now = DateTime.UtcNow;
        var cardIds = rememberItems.Select(c => c.CardId).ToList();

        var collection = await db.Collections.FindAsync(userId, collectionId);

        if (collection == null)
        {
            return (false, "card's collection not found", null);
        }

        var queueItems = await db.Queue
            .Where(q => q.ParentUserId == userId
                        && q.ParentCollectionId == collectionId
                        && cardIds.Contains(q.ParentCardId)
                        && q.Date.Date == date.Date)
            .ToListAsync();

        if (queueItems.Count == 0 || queueItems.Count != cardIds.Count)
            return (false, "Incorrect request", null);

        var cards = await db.Cards
            .Where(c => c.ParentUserId == userId
                        && c.ParentCollectionId == collectionId
                        && cardIds.Contains(c.Id))
            .ToListAsync();

        var scheduleIds = cards.Select(c => c.ParentRepeatsScheduleId).ToList();

        var schedules = await db.RepeatsSchedules
            .Where(s => s.ParentUserId == userId && scheduleIds.Contains(s.Id))
            .Include(s => s.Phases)
            .AsSplitQuery()
            .ToListAsync();

        var metadata = metadataService.GetMetadata(userId);
        var closestRepeatDate = DateTime.MaxValue;

        foreach (var rememberItem in rememberItems)
        {
            var cardId = rememberItem.CardId;
            var weight = rememberItem.Weight;

            var queueItem = queueItems.Single(q => q.ParentCardId == cardId);
            var card = cards.Single(c => c.Id == cardId);
            var schedule = schedules.Single(s => s.Id == card.ParentRepeatsScheduleId);

            var remember = new RememberEntity(
                userId,
                collectionId,
                cardId,
                weight,
                queueItem.PhaseStep,
                now
            );

            db.Entry(remember).State = EntityState.Added;
            
            if (remember.PhaseStep >= schedule.Phases.Count)
            {
                metadataService.CardStateChanged(metadata, collection, card.IsFinished, true);
                card.IsFinished = true;
                continue;
            }

            var nextPhaseIndex = remember.PhaseStep;
            var nextPhase = schedule.Phases[nextPhaseIndex];
            var nextRepeatDate = now.AddSeconds(nextPhase.SecondsFromLastPhase);

            if (nextRepeatDate < closestRepeatDate)
                closestRepeatDate = nextRepeatDate;

            var newQueueItem = new CardRepeatQueueEntity(
                userId,
                collectionId,
                cardId,
                (short) (nextPhaseIndex + 1),
                nextRepeatDate);

            db.Entry(queueItem).State = EntityState.Deleted;
            db.Entry(newQueueItem).State = EntityState.Added;
            db.Entry(metadata).State = EntityState.Modified;
            db.Entry(collection).State = EntityState.Modified;
        }


        try
        {
            db.SaveChanges();
            return (true, null, closestRepeatDate == DateTime.MaxValue ? null : closestRepeatDate);
        }
        catch
        {
            return (false, "unknown error", null);
        }
    }

    public class RememberItem
    {
        public short CardId { get; }
        public float Weight { get; }

        public RememberItem(short cardId, float weight)
        {
            CardId = cardId;
            Weight = weight;
        }
    }
}