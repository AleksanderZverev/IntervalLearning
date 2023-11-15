using System.Diagnostics;
using System.Linq.Expressions;
using DB;
using DB.Configurations.Study;
using DB.Models;
using Domain.Card;
using Domain.Card.ValueObjects;
using Domain.Collection.ValueObjects;
using Domain.User.ValueObjects;
using IntervalLearningApi.Models;
using Microsoft.EntityFrameworkCore;

namespace IntervalLearningApi.Services;

public record LearningStatistic(
    int RepeatedCards,
    int LearnedCards
);

public class CardsService
{
    private readonly ILogger<CardsService> logger;
    private readonly IHostEnvironment env;
    private readonly ApplicationContext db;

    public CardsService(ILogger<CardsService> logger,
        IHostEnvironment env,
        ApplicationContext db)
    {
        this.logger = logger;
        this.env = env;
        this.db = db;
    }

    public Task<List<Card>> GetAllCards(UserId userId, CollectionId collectionId)
    {
        return db.Cards
            .Where(c => c.ParentUserId == userId && c.ParentCollectionId == collectionId)
            .ToListAsync();
    }

    public Task<Card?> FindCard(UserId userId, CollectionId collectionId, CardId cardId)
    {
        return db.Cards
            .Include(r => r.Remembers)
            .AsSplitQuery()
            .SingleOrDefaultAsync(c => c.ParentUserId == userId && c.ParentCollectionId == collectionId && c.Id == cardId);
    }

    public Task<List<Card>> GetCards(UserId userId, CollectionId collectionId, int page, int count)
    {
        var toSkip = (page - 1) * count;

        return db.Cards
            .Where(c => c.ParentUserId == userId && c.ParentCollectionId == collectionId)
            .OrderByDescending(c => c.CreatedDate)
            .Skip(toSkip)
            .Take(count)
            .ToListAsync();
    }

    public (Card? card, string? error, bool isCreated) CreateOrEdit(CreateOrPatchCard item, CardId? cardId)
    {
        return cardId == null
            ? Create(item)
            : Edit(item, cardId);
    }
    
    public (Card? card, string? error, bool isCreated) Edit(CreateOrPatchCard item, CardId cardId)
    {
        var card = db.Cards.Find(item.ParentUserId, item.ParentCollectionId, cardId);

        if (card == null)
            return (null, "Card not found", false);
        
        card.MeaningText = item.MeaningText;
        card.RememberingText = item.RememberingText;
        card.PromptText = item.PromptText;
        card.Description = item.Description;
        card.Examples = item.Examples;

        try
        {
            db.Update(card);
            db.SaveChanges();
            return (card, null, false);
        }
        catch
        {
            return (null, "Unknown error", false);
        }
    }
    
    public (Card? card, string? error, bool isCreated) Create(CreateOrPatchCard item)
    {
        var sequenceName = CardConfiguration.GetSequenceName(
            item.ParentUserId,
            item.ParentCollectionId);
        
        db.EnsureSequenceCreated(sequenceName);
        var nextCardId = db.GetSequenceNextValue16(sequenceName);
        var cardId = CardId.Create(nextCardId).Value;
        var card = new Card(item.ParentUserId, item.ParentCollectionId, cardId)
        {
            MeaningText = item.MeaningText,
            RememberingText = item.RememberingText,
            PromptText = item.PromptText,
            Description = item.Description,
        };
        
        if (item.Examples is { Count: > 0 })
        {
            card.Examples = item.Examples;
        }

        try
        {
            db.Add(card);
            db.SaveChanges();
            return (card, null, true);
        }
        catch
        {
            return (null, "Unknown error", false);
        }
    }
    
    public async Task<(Card? card, string? error)> Delete(UserId userId, CollectionId collectionId, CardId cardId)
    {
        var card = await db.Cards.FindAsync(userId, collectionId, cardId);

        if (card == null)
            return (null, "Card not found");

        var deletedCard = db.Cards.Remove(card);
        try
        {
            await db.SaveChangesAsync();
            return (deletedCard.Entity, null);
        }
        catch
        {
            return (null, "Unable to save changes to database");
        }
    }

    public async Task<List<Card>> Search(
        UserId userId,
        CollectionId collectionId,
        string searchValue,
        SearchFieldType fieldType,
        int page,
        int count)
    {
        var skip = (page - 1) * count;

        return fieldType switch
        {
            SearchFieldType.RememberingText => await GetCards(c =>
                c.ParentUserId == userId
                && c.ParentCollectionId == collectionId
                && EF.Functions.ILike(c.RememberingText, $"{searchValue}%"), skip, count),
            SearchFieldType.PromptText => await GetCards(c =>
                c.ParentUserId == userId
                && c.ParentCollectionId == collectionId
                && EF.Functions.ILike(c.PromptText, $"{searchValue}%"), skip, count),
            SearchFieldType.MeaningText => await GetCards(c =>
                c.ParentUserId == userId
                && c.ParentCollectionId == collectionId
                && EF.Functions.ILike(c.MeaningText, $"{searchValue}%"), skip, count),
            _ => throw new ArgumentOutOfRangeException(nameof(fieldType), fieldType, null)
        };
    }

    private async Task<List<Card>> GetCards(Expression<Func<Card, bool>> condition, int skip, int take)
    {
        return await db.Cards
            .Where(condition)
            .OrderByDescending(c => c.CreatedDate)
            .Skip(skip)
            .Take(take)
            .ToListAsync();
    }

    public async Task<(Card? card, string? error, bool isMoved)> MoveCard(
        UserId userId,
        CollectionId sourceCollectionId,
        CollectionId destinationCollectionId,
        CardId cardId,
        bool disableTransaction = false)
    {
        var card = await db.Cards
            .Include(c => c.Remembers)
            .AsSplitQuery()
            .SingleOrDefaultAsync(c =>
                c.ParentUserId == userId && c.ParentCollectionId == sourceCollectionId && c.Id == cardId);
        
        if (!disableTransaction)
            await db.Database.BeginTransactionAsync();

        var movedCard = new Card(userId, destinationCollectionId, cardId)
        {
            RememberingText = card.RememberingText,
            PromptText = card.PromptText,
            MeaningText = card.MeaningText,
            Description = card.Description,
            Examples = card.Examples is {Count: >0} 
                ? card.Examples.ToList() 
                : new List<CardExample>(),
            CreatedDate = card.CreatedDate,
        };

        db.Add(movedCard);

        if (!db.SoftSaveChanges())
        {
            if (!disableTransaction)
                await db.Database.RollbackTransactionAsync();
            return (null, "Unable to create card", false);
        }
        
        var remembers = card.Remembers.Select(r => new RememberEntity(
            r.ParentRepeatsScheduleUserId,
            r.ParentRepeatsScheduleId,
            movedCard.ParentUserId,
            movedCard.ParentCollectionId,
            movedCard.Id,
            r.Weight,
            r.PhaseIndex,
            r.RepeatedDate)).ToList();
        
        await db.Remembers.AddRangeAsync(remembers);
        
        if (!db.SoftSaveChanges())
        {
            return (null, "Unable to save remember entities", false);
        }

        var (deleted, deletionError) = await Delete(userId, sourceCollectionId, cardId);

        if (deletionError != null)
        {
            return (null, deletionError, false);
        }

        if (!disableTransaction)
            await db.Database.CommitTransactionAsync();

        return (movedCard, null, true);
    }

    public async Task<(List<Card>? cards, string? error)> GetNotStartedCards(
        UserId scheduleUserId,
        short scheduleId,
        UserId userId,
        CollectionId collectionId, 
        int count)
    {
        var schedule = await db.RepeatsSchedules.FindAsync(scheduleUserId, scheduleId);

        if (schedule == null)
        {
            return (null, "schedule not found");
        }

        var startedCardIds = await db.Remembers
            .Where(r => r.ParentUserId == userId
                        && r.ParentCollectionId == collectionId
                        && r.ParentRepeatsScheduleUserId == scheduleUserId
                        && r.ParentRepeatsScheduleId == scheduleId)
            .Select(c => c.ParentCardId)
            .ToListAsync();

        var canStartCards = await db.Cards
            .Where(c => c.ParentUserId == userId 
                        && c.ParentCollectionId == collectionId 
                        && !startedCardIds.Contains(c.Id))
            .OrderBy(c => c.Id)
            .Take(count)
            .ToListAsync();

        return (canStartCards, null);
    }

    public async Task<List<Card>> GetCardsQueue(
        UserId userId,
        CollectionId collectionId,
        UserId scheduleUserId,
        short scheduleId,
        short phaseIndex, 
        DateTime dateTime)
    {
        if (env.IsProduction() && dateTime.Date > DateTime.UtcNow.Date)
            return new List<Card>();

        var queueItems = await db.Queue
            .Where(c => c.ParentUserId == userId 
                        && c.ParentCollectionId == collectionId
                        && c.ParentRepeatsScheduleUserId == scheduleUserId
                        && c.ParentRepeatsScheduleId == scheduleId
                        && c.PhaseIndex == phaseIndex
                        && c.Date.Date == dateTime.Date)
            .ToListAsync();

        if (queueItems.Count == 0)
            return new List<Card>(0);

        var cardsIds = queueItems.Select(q => q.ParentCardId).ToList();

        var cards = await db.Cards
            .Where(c => c.ParentUserId == userId
                        && c.ParentCollectionId == collectionId
                        && cardsIds.Contains(c.Id))
            .ToListAsync();

        var remembers = await db.Remembers.Where(r => r.ParentUserId == userId
                                                      && r.ParentCollectionId == collectionId
                                                      && r.ParentRepeatsScheduleUserId == scheduleUserId
                                                      && r.ParentRepeatsScheduleId == scheduleId
                                                      && cardsIds.Contains(r.ParentCardId))
            .ToListAsync();

        var cardIdToRemember = remembers
            .GroupBy(r => r.ParentCardId)
            .ToDictionary(g => g.Key, g => g.ToList());

        foreach (var card in cards)
        {
            var cardsRemembers = cardIdToRemember[card.Id];
            card.Remembers = cardsRemembers;
        }

        return cards;
    }

    public (NextRepeatInfo? closestRepeatInfo, string? reason) Start(
        UserId userId,
        CollectionId collectionId,
        UserId scheduleUserId,
        short scheduleId, 
        List<short> cardIds)
    {
        var schedule = db.RepeatsSchedules
            .Include(s => s.Phases)
            .AsSplitQuery()
            .SingleOrDefault(s => s.ParentUserId == scheduleUserId && s.Id == scheduleId);

        if (schedule == null)
        {
            return (null, "Schedule not found");
        }

        if (schedule.Phases.Count == 0)
        {
            return (null, "No phases found");
        }

        var startedCards = db.Cards.Where(c => 
                c.ParentUserId == userId 
                && c.ParentCollectionId == collectionId
                && cardIds.Contains(c.Id))
            .Include(c => c.Remembers)
            .AsSplitQuery()
            .ToList();

        if (startedCards.Count == 0)
        {
            return (null, "No cards");
        }

        db.Database.BeginTransaction();

        startedCards.ForEach(c =>
        {
            var remember = new RememberEntity(scheduleUserId, scheduleId, userId, collectionId, c.Id, 1f, -1, DateTime.UtcNow);
            db.Entry(remember).State = EntityState.Added;
        });

        try
        {
            db.SaveChanges();
        }
        catch (Exception e)
        {
            logger.LogError(e.ToString());
            db.Database.RollbackTransaction();
            return (null, "unknown error");
        }

        var (nextRepeatInfo, queueError) = AddToQueue(userId, collectionId, startedCards, schedule);

        if (nextRepeatInfo == null)
        {
            db.Database.RollbackTransaction();
            return (null, queueError);
        }

        db.Database.CommitTransaction();

        return (nextRepeatInfo, null);
    }

    private (NextRepeatInfo? closestRepeatInfo, string? reason) AddToQueue(
        UserId userId, 
        CollectionId collectionId, 
        List<Card> cards,
        RepeatsScheduleEntity scheduleWithPhases)
    {
        var closestRepeatDate = DateTime.MaxValue;
        var closestPhaseIndex = -1;
        PhaseEntity? closestPhaseInfo = null;
        var queueItems = new List<CardRepeatQueueEntity>(cards.Count);

        foreach (var card in cards)
        {
            var lastRemember = card.FindLastRemember();
            var nextPhaseIndex = lastRemember == null ? 0 : lastRemember.PhaseIndex + 1;
            var nextPhase = scheduleWithPhases.FindPhase(nextPhaseIndex);

            if (nextPhase == null)
            {
                Debug.Fail("nextPhase == null");
                return (null, "An error in algorithm work");
            }

            var nextRepeatDate = nextPhase.GetNextDate(DateTime.UtcNow);

            var queueItem = new CardRepeatQueueEntity(
                scheduleWithPhases.ParentUserId,
                scheduleWithPhases.Id,
                userId,
                collectionId,
                card.Id,
                (short)nextPhaseIndex,
                nextRepeatDate
            );

            queueItems.Add(queueItem);
            
            if (nextRepeatDate <= closestRepeatDate)
            {
                closestRepeatDate = nextRepeatDate;
                closestPhaseInfo = nextPhase;
                closestPhaseIndex = nextPhaseIndex;
            }
        }

        queueItems.ForEach(q => db.Entry(q).State = EntityState.Added);

        try
        {
            db.SaveChanges();
        }
        catch (Exception e)
        {
            logger.LogError(e.ToString());
            return (null, "unknown error");
        }

#if DEBUG
        if (closestRepeatDate == DateTime.MaxValue)
            Debug.Fail("closestRepeatDate == DateTime.MaxValue)");
#endif

        return (new NextRepeatInfo(closestRepeatDate, closestPhaseInfo, closestPhaseIndex), null);
    }

    public async Task<(NextRepeatInfo? closestRepeatInfo, string? reason)> Remember(
        UserId userId,
        CollectionId collectionId,
        UserId scheduleUserId,
        short scheduleId,
        short phaseIndex,
        List<RememberItem> rememberItems)
    {
        var now = DateTime.UtcNow;
        var cardIds = rememberItems.ConvertAll(c => c.CardId);

        var collection = await db.Collections.FindAsync(userId, collectionId);

        if (collection == null)
        {
            return (null, "card's collection not found");
        }

        var queueItems = await db.Queue
            .Where(q => q.ParentUserId == userId
                        && q.ParentCollectionId == collectionId
                        && q.ParentRepeatsScheduleUserId == scheduleUserId
                        && q.ParentRepeatsScheduleId == scheduleId
                        && q.PhaseIndex == phaseIndex
                        && cardIds.Contains(q.ParentCardId))
            .ToListAsync();

        if (queueItems.Count == 0 || queueItems.Count != cardIds.Count)
            return (null, "Incorrect request");

        var schedule = db.RepeatsSchedules
            .Include(s => s.Phases)
            .SingleOrDefault(s => s.ParentUserId == scheduleUserId && s.Id == scheduleId);

        if (schedule == null)
        {
            return (null, "Schedule not found");
        }
        
        await using var transaction = await db.Database.BeginTransactionAsync();
        
        var closestRepeatDate = DateTime.MaxValue;
        var closestPhaseIndex = -1;
        PhaseEntity? closestPhaseInfo = null;

        var forbidDate = DateTime.UtcNow.Date.AddDays(1);

        foreach (var rememberItem in rememberItems)
        {
            var cardId = CardId.Create(rememberItem.CardId).Value;
            var weight = rememberItem.Weight;

            var queueItem = queueItems.Single(q => q.ParentCardId == cardId);

            if (queueItem.Date.Date >= forbidDate && env.IsProduction())
            {
                logger.LogInformation("Unable to remember. Not time!");
                return (null, "unable to repeat");
            }

            var remember = new RememberEntity(
                schedule.ParentUserId,
                schedule.Id,
                userId,
                collectionId,
                cardId,
                weight,
                queueItem.PhaseIndex,
                now
            );

            db.Entry(remember).State = EntityState.Added;

            var currentPhase = schedule.GetPhase(queueItem.PhaseIndex);

            var phaseRemember = new PhaseRememberEntity(
                schedule.ParentUserId,
                schedule.Id,
                currentPhase.Id,
                userId,
                weight);

            db.Entry(phaseRemember).State = EntityState.Added;
            db.Entry(queueItem).State = EntityState.Deleted;

            var card = await FindCard(userId, collectionId, cardId);

            if (card == null)
            {
                return (null, "Internal error, card not found");
            }
            
            var (nextPhaseIndex, nextPhase) = schedule.GetNextPhaseIndex(card, remember);

            if (nextPhase == null)
                continue;

            var nextRepeatDate = nextPhase.GetNextDate(now);

            var newQueueItem = new CardRepeatQueueEntity(
                schedule.ParentUserId,
                schedule.Id,
                userId,
                collectionId,
                cardId,
                (short)nextPhaseIndex,
                nextRepeatDate);

            db.Entry(newQueueItem).State = EntityState.Added;
            
            if (nextRepeatDate < closestRepeatDate)
            {
                closestRepeatDate = nextRepeatDate;
                closestPhaseInfo = nextPhase;
                closestPhaseIndex = nextPhaseIndex;
            }
        }

        var isOk = db.SoftSaveChanges();

        if (!isOk)
            return (null, "unknown error");

        await transaction.CommitAsync();

        return (new NextRepeatInfo(
            closestRepeatDate == DateTime.MaxValue ? null : closestRepeatDate,
            closestPhaseInfo,
            closestPhaseIndex), null);
    }

    public class CreateOrPatchCard
    {
        public CardText RememberingText { get; init; }
        public CardText? PromptText { get; init; }
        public CardText MeaningText { get; init; }
        public CardDescription? Description { get; init; }
        public List<CardExample> Examples { get; init; }
        public UserId ParentUserId { get; init; }
        public CollectionId ParentCollectionId { get; init; }
    }

    public class RememberItem
    {
        public required  CardId CardId { get; init; }
        public required float Weight { get; init; }
    }

    public class NextRepeatInfo
    {
        public DateTime? NextRepeatDate { get; }
        public int NextPhaseIndex { get; }
        public PhaseEntity? NextPhase { get; }

        public NextRepeatInfo(DateTime? nextRepeatDate, PhaseEntity? nextPhase, int nextPhaseIndex)
        {
            NextRepeatDate = nextRepeatDate;
            NextPhase = nextPhase;
            NextPhaseIndex = nextPhaseIndex;
        }
    }
}