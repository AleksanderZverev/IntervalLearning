using System.Diagnostics;
using System.Linq.Expressions;
using Application.Common.Interfaces.DB.Transactions;
using DB;
using DB.Configurations.Study;
using DB.Models;
using DB.Models.ValueObjects;
using Domain.Card;
using Domain.Card.ValueObjects;
using Domain.Collection.ValueObjects;
using Domain.Queue;
using Domain.Schedule;
using Domain.Schedule.Entities.Remember;
using Domain.User.ValueObjects;
using FluentResults;
using Infrastructure.Errors;
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
    private readonly ITransactionProvider transactionProvider;

    public CardsService(ILogger<CardsService> logger,
        IHostEnvironment env,
        ApplicationContext db,
        ITransactionProvider transactionProvider)
    {
        this.logger = logger;
        this.env = env;
        this.db = db;
        this.transactionProvider = transactionProvider;
    }

    public Task<List<Card>> GetAllCards(UserId userId, CollectionId collectionId)
    {
        return db.Cards
            .Where(c => c.ParentUserId == userId && c.ParentCollectionId == collectionId)
            .ToListAsync();
    }

    private Task<Card?> FindCard(UserId userId, CollectionId collectionId, CardId cardId)
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

    public Result<Card> Edit(CreateOrPatchCard item, CardId cardId)
    {
        var card = db.Cards.Find(item.ParentUserId, item.ParentCollectionId, cardId);

        if (card == null)
            return new NotFoundError(nameof(Card));
        
        card.MeaningText = item.MeaningText;
        card.RememberingText = item.RememberingText;
        card.PromptText = item.PromptText;
        card.Description = item.Description;
        card.Examples = item.Examples;

        db.Update(card);
        return db.SoftSaveChanges()
            ? card
            : new InternalError();
    }
    
    public Result<Card> Create(CreateOrPatchCard item)
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

        db.Add(card);
        
        return db.SoftSaveChanges()
            ? card
            : new InternalError();
    }
    
    public async Task<Result<Card>> Delete(UserId userId, CollectionId collectionId, CardId cardId)
    {
        var card = await db.Cards.FindAsync(userId, collectionId, cardId);

        if (card == null)
            return new NotFoundError(nameof(Card));

        var deletedCard = db.Cards.Remove(card);
        return await db.SoftSaveChangesAsync()
            ? deletedCard.Entity
            : new InternalError();
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

    public async Task<Result<Card>> MoveCard(
        UserId userId,
        CollectionId sourceCollectionId,
        CollectionId destinationCollectionId,
        CardId cardId)
    {
        var card = await db.Cards
            .Include(c => c.Remembers)
            .AsSplitQuery()
            .SingleOrDefaultAsync(c =>
                c.ParentUserId == userId && c.ParentCollectionId == sourceCollectionId && c.Id == cardId);

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
        
        using var transaction = transactionProvider.CreateScope();
        
        db.Add(movedCard);
        
        if (!await db.SoftSaveChangesAsync())
        {
            return new Error("Failure on creating card");
        }
        
        var remembers = card.Remembers.Select(r => new Remember(
            r.ParentRepeatsScheduleUserId,
            r.ParentRepeatsScheduleId,
            movedCard.ParentUserId,
            movedCard.ParentCollectionId,
            movedCard.Id,
            r.Id,
            r.Weight,
            r.PhaseIndex,
            r.RepeatedDate)).ToList();
        
        await db.Remembers.AddRangeAsync(remembers);
        
        if (!await db.SoftSaveChangesAsync())
        {
            return new Error("Failure on saving remember entities");
        }

        var deletionResult = await Delete(userId, sourceCollectionId, cardId);

        if (deletionResult.IsFailed)
        {
            return deletionResult;
        }

        transaction.Complete();
        return movedCard;
    }

    public async Task<Result<List<Card>>> GetNotStartedCards(
        UserId scheduleUserId,
        ScheduleId scheduleId,
        UserId userId,
        CollectionId collectionId, 
        int count)
    {
        var schedule = await db.RepeatsSchedules.FindAsync(scheduleUserId, scheduleId);

        if (schedule == null)
        {
            return new NotFoundError(nameof(schedule));
        }

        //GetRangeForCollection
        var startedCardIds = await db.Remembers
            .Where(r => r.ParentUserId == userId
                        && r.ParentCollectionId == collectionId
                        && r.ParentRepeatsScheduleUserId == scheduleUserId
                        && r.ParentRepeatsScheduleId == scheduleId)
            .Select(c => c.ParentCardId)
            .ToListAsync();

        //GetExceptRange
        var canStartCards = await db.Cards
            .Where(c => c.ParentUserId == userId 
                        && c.ParentCollectionId == collectionId 
                        && !startedCardIds.Contains(c.Id))
            .OrderBy(c => c.Id)
            .Take(count)
            .ToListAsync();

        return canStartCards;
    }

    public async Task<List<Card>> GetCardsQueue(
        UserId userId,
        CollectionId collectionId,
        UserId scheduleUserId,
        ScheduleId scheduleId,
        short phaseIndex, 
        DateTime dateTime)
    {
        //GetByDate
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

        //GetRange
        var cards = await db.Cards
            .Where(c => c.ParentUserId == userId
                        && c.ParentCollectionId == collectionId
                        && cardsIds.Contains(c.Id))
            .ToListAsync();

        //GetRange
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

    public Result<NextRepeatInfo> Start(
        UserId userId,
        CollectionId collectionId,
        UserId scheduleUserId,
        ScheduleId scheduleId, 
        List<short> cardIds)
    {
        var schedule = db.RepeatsSchedules
            .Include(s => s.Phases)
            .AsSplitQuery()
            .SingleOrDefault(s => s.ParentUserId == scheduleUserId && s.Id == scheduleId);

        if (schedule == null)
        {
            return new NotFoundError("Schedule");
        }

        if (schedule.Phases.Count == 0)
        {
            return new NotFoundError("Phases");
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
            return new NotFoundError("Cards");
        }

        using var transaction = transactionProvider.CreateScope();

        var startedDate = DateTime.UtcNow;
        startedCards.ForEach(c =>
        {
            var remember = CreateRemember(schedule, c, RememberWeight.Create(1f).Value, -1, startedDate);
            db.Entry(remember).State = EntityState.Added;
        });

        if (!db.SoftSaveChanges())
        {
            return new InternalError();
        }

        var nextRepeatInfoResult = AddToQueue(userId, collectionId, startedCards, schedule);

        if (nextRepeatInfoResult.IsFailed)
        {
            return nextRepeatInfoResult;
        }

        transaction.Complete();
        return nextRepeatInfoResult.Value;
    }

    private Result<NextRepeatInfo> AddToQueue(
        UserId userId, 
        CollectionId collectionId, 
        List<Card> cards,
        RepeatsSchedule scheduleWithPhases)
    {
        var closestRepeatDate = DateTime.MaxValue;
        var closestPhaseIndex = -1;
        Phase? closestPhaseInfo = null;
        var queueItems = new List<CardRepeatQueue>(cards.Count);

        foreach (var card in cards)
        {
            var (startPhase, phaseIndex) = scheduleWithPhases.FindFirstPhase();

            if (startPhase == null)
            {
                Debug.Fail("An error in algorithm work: nextPhase == null");
                return new InternalError();
            }

            var nextRepeatDate = startPhase.GetNextDate(DateTime.UtcNow);
            var nextQueueItem = GetNextQueue(
                scheduleWithPhases,
                card,
                phaseIndex,
                nextRepeatDate);
            queueItems.Add(nextQueueItem);
            
            if (nextRepeatDate <= closestRepeatDate)
            {
                closestRepeatDate = nextRepeatDate;
                closestPhaseInfo = startPhase;
                closestPhaseIndex = phaseIndex;
            }
        }

        queueItems.ForEach(q => db.Entry(q).State = EntityState.Added);

        if (!db.SoftSaveChanges())
        {
            return new InternalError();
        }

        Debug.Assert(closestRepeatDate != DateTime.MaxValue, "closestRepeatDate != DateTime.MaxValue");
        return new NextRepeatInfo(closestRepeatDate, closestPhaseInfo, closestPhaseIndex);
    }

    private CardRepeatQueue GetNextQueue(RepeatsSchedule scheduleWithPhases, Card card, int nextPhaseIndex, DateTime nextRepeatDate)
    {
        var queueSequenceName = CardRepeatQueueConfiguration.GetSequenceName(scheduleWithPhases, card);
        db.EnsureSequenceCreated(queueSequenceName);
        var nextValue = db.GetSequenceNextValue16(queueSequenceName);
        var queueId = QueueId.Create(nextValue).Value;
        
        var queueItem = new CardRepeatQueue(
            scheduleWithPhases.ParentUserId,
            scheduleWithPhases.Id,
            card.ParentUserId,
            card.ParentCollectionId,
            card.Id,
            queueId,
            (short)nextPhaseIndex,
            nextRepeatDate
        );
        return queueItem;
    }

    public async Task<Result<NextRepeatInfo>> Remember(
        UserId userId,
        CollectionId collectionId,
        UserId scheduleUserId,
        ScheduleId scheduleId,
        short phaseIndex,
        List<RememberItem> rememberItems)
    {
        var cardIds = rememberItems.ConvertAll(c => c.CardId);

        var collection = await db.Collections.FindAsync(userId, collectionId);

        if (collection == null)
        {
            return new NotFoundError("card's collection");
        }

        //GetForCards
        var queueItems = await db.Queue
            .Where(q => q.ParentUserId == userId
                        && q.ParentCollectionId == collectionId
                        && q.ParentRepeatsScheduleUserId == scheduleUserId
                        && q.ParentRepeatsScheduleId == scheduleId
                        && q.PhaseIndex == phaseIndex
                        && cardIds.Contains(q.ParentCardId))
            .ToListAsync();

        if (queueItems.Count == 0 || queueItems.Count != cardIds.Count)
            return new BadRequestError();

        var schedule = db.RepeatsSchedules
            .Include(s => s.Phases)
            .SingleOrDefault(s => s.ParentUserId == scheduleUserId && s.Id == scheduleId);

        if (schedule == null)
        {
            return new NotFoundError("Schedule");
        }
        
        using var transaction = transactionProvider.CreateScope();
        
        var closestRepeatDate = DateTime.MaxValue;
        var closestPhaseIndex = -1;
        Phase? closestPhaseInfo = null;

        var now = DateTime.UtcNow;
        var forbidDate = now.Date.AddDays(1);

        foreach (var rememberItem in rememberItems)
        {
            var weight = rememberItem.Weight;
            var cardId = CardId.Create(rememberItem.CardId).Value;
            var card = await FindCard(userId, collectionId, cardId);

            if (card == null)
            {
                return new InternalError();
            }

            var queueItem = queueItems.Single(q => q.ParentCardId == cardId);

            if (queueItem.Date.Date >= forbidDate && env.IsProduction())
            {
                logger.LogInformation("Unable to remember. Not time!");
                return new BadRequestError("It's too early to repeat now");
            }

            var remember = CreateRemember(schedule, card, weight, queueItem.PhaseIndex, now);
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

            var (nextPhaseIndex, nextPhase) = schedule.GetNextPhase(card, remember);

            if (nextPhase == null)
                continue;

            var nextRepeatDate = nextPhase.GetNextDate(now);
            var newQueueItem = GetNextQueue(schedule, card, nextPhaseIndex, nextRepeatDate);
            db.Entry(newQueueItem).State = EntityState.Added;
            
            if (nextRepeatDate < closestRepeatDate)
            {
                closestRepeatDate = nextRepeatDate;
                closestPhaseInfo = nextPhase;
                closestPhaseIndex = nextPhaseIndex;
            }
        }

        if (!await db.SoftSaveChangesAsync())
            return new InternalError();

        transaction.Complete();

        return new NextRepeatInfo(
            closestRepeatDate == DateTime.MaxValue ? null : closestRepeatDate,
            closestPhaseInfo,
            closestPhaseIndex);
    }

    private Remember CreateRemember(RepeatsSchedule schedule, Card card, RememberWeight weight, int phaseIndex, DateTime date)
    {
        var sequenceName = RememberConfiguration.GetSequenceName(
            new ComplexScheduleId()
            {
                ParentUserId = schedule.ParentUserId,
                Id = schedule.Id,
            },
            new ComplexCardId()
            {
                UserId = card.ParentUserId,
                CollectionId = card.ParentCollectionId,
                Id = card.Id,
            });
        
        db.EnsureSequenceCreated(sequenceName);
        var nextValue = db.GetSequenceNextValue16(sequenceName);
        var rememberId = RememberId.Create(nextValue).Value;
        return new Remember(
            schedule.ParentUserId, 
            schedule.Id,
            card.ParentUserId,
            card.ParentCollectionId,
            card.Id,
            rememberId,
            weight, 
            (short)phaseIndex,
            date);
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
        public required CardId CardId { get; init; }
        public required RememberWeight  Weight { get; init; }
    }

    public class NextRepeatInfo
    {
        public DateTime? NextRepeatDate { get; }
        public int NextPhaseIndex { get; }
        public Phase? NextPhase { get; }

        public NextRepeatInfo(DateTime? nextRepeatDate, Phase? nextPhase, int nextPhaseIndex)
        {
            NextRepeatDate = nextRepeatDate;
            NextPhase = nextPhase;
            NextPhaseIndex = nextPhaseIndex;
        }
    }
}