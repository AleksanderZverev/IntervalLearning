using System.Diagnostics;
using System.Linq.Expressions;
using DB;
using DB.Models;
using Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace IntervalLearningApi.Services;

public class CardsService
{
    private readonly ILogger<CardsService> logger;
    private readonly IWebHostEnvironment env;
    private readonly ApplicationContext db;

    public CardsService(ILogger<CardsService> logger,
        IWebHostEnvironment env,
        ApplicationContext db)
    {
        this.logger = logger;
        this.env = env;
        this.db = db;
    }

    public Task<List<CardEntity>> GetAllCards(long userId, short collectionId)
    {
        return db.Cards
            .Where(c => c.ParentUserId == userId && c.ParentCollectionId == collectionId)
            .ToListAsync();
    }

    public Task<List<CardEntity>> GetCards(long userId, short collectionId, int page, int count)
    {
        var toSkip = (page - 1) * count;

        return db.Cards
            .Where(c => c.ParentUserId == userId && c.ParentCollectionId == collectionId)
            .OrderByDescending(c => c.CreatedDate)
            .Skip(toSkip)
            .Take(count)
            .ToListAsync();
    }

    public (CardEntity? card, string? error, bool isCreated) CreateOrEdit(CreateOrPatchCard item, short? cardId)
    {
        var card = cardId == null
            ? new CardEntity()
            : db.Cards.Find(item.ParentUserId, item.ParentCollectionId, cardId);

        if (card == null)
            return (null, "Card not found", false);

        var entry = db.Entry(card);
        entry.CurrentValues.SetValues(item);

        try
        {
            var isCreating = cardId == null;

            if (isCreating)
                entry.State = EntityState.Added;

            db.SaveChanges();
            return (card, null, isCreating);
        }
        catch
        {
            return (null, "Unknown error", false);
        }
    }
    
    public async Task<(CardEntity? card, string? error)> Delete(long userId, short collectionId, short cardId)
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

    public async Task<List<CardEntity>> Search(long userId, short collectionId, string searchValue)
    {
        foreach (var getValue in new Expression<Func<CardEntity, bool>>[]
                 {
                     c => c.ParentUserId == userId && c.ParentCollectionId == collectionId && c.FrontSideText == searchValue,
                     c => c.ParentUserId == userId && c.ParentCollectionId == collectionId && c.PromptText == searchValue,
                     c => c.ParentUserId == userId && c.ParentCollectionId == collectionId && c.BackSideText == searchValue
                 })
        {
            var cards = await GetCards(getValue);
            if (cards.Count != 0)
                return cards;
        }

        return new List<CardEntity>();
    }

    private async Task<List<CardEntity>> GetCards(Expression<Func<CardEntity, bool>> condition)
    {
        return await db.Cards
            .Where(condition)
            .OrderByDescending(c => c.CreatedDate)
            .ToListAsync();
    }

    public async Task<(List<CardEntity>? cards, string? error)> GetNotStartedCards(long scheduleUserId,
        short scheduleId,
        long userId,
        short collectionId, 
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

    public async Task<List<CardEntity>> GetCardsQueue(long userId,
        short collectionId,
        long scheduleUserId,
        short scheduleId,
        short phaseIndex, 
        DateTime dateTime)
    {
        if (env.IsProduction() && dateTime.Date > DateTime.UtcNow.Date)
            return new List<CardEntity>();

        var queueItems = await db.Queue
            .Where(c => c.ParentUserId == userId 
                        && c.ParentCollectionId == collectionId
                        && c.ParentRepeatsScheduleUserId == scheduleUserId
                        && c.ParentRepeatsScheduleId == scheduleId
                        && c.PhaseIndex == phaseIndex
                        && c.Date.Date == dateTime.Date)
            .ToListAsync();

        if (queueItems.Count == 0)
            return new List<CardEntity>(0);

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

        foreach (var cardEntity in cards)
        {
            var cardsRemembers = cardIdToRemember[cardEntity.Id];
            cardEntity.Remembers = cardsRemembers;
        }

        return cards;
    }

    public (NextRepeatInfo? closestRepeatInfo, string? reason) Start(long userId,
        short collectionId,
        long scheduleUserId,
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
        long userId, 
        short collectionId, 
        List<CardEntity> cards,
        RepeatsScheduleEntity scheduleWithPhases)
    {
        var closestRepeatDate = DateTime.MaxValue;
        var closestPhaseIndex = -1;
        PhaseEntity? closestPhaseInfo = null;
        var queueItems = new List<CardRepeatQueueEntity>(cards.Count);

        foreach (var card in cards)
        {
            var lastRemember = card.Remembers.MaxBy(c => c.Id);
            var nextPhaseIndex = lastRemember == null ? 0 : lastRemember.PhaseIndex + 1;
            var nextPhase = scheduleWithPhases.Phases
                .OrderBy(p => p.Id)
                .Skip(nextPhaseIndex)
                .FirstOrDefault();

            if (nextPhase == null)
            {
                Debug.Fail("nextPhase == null");
                return (null, "Error in algorithm work");
            }

            var nextRepeatDate = DateTime.UtcNow.AddSeconds(nextPhase.SecondsFromLastPhase);

            if (nextRepeatDate <= closestRepeatDate)
            {
                closestRepeatDate = nextRepeatDate;
                closestPhaseInfo = nextPhase;
                closestPhaseIndex = nextPhaseIndex;
            }

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

    public async Task<(NextRepeatInfo? closestRepeatInfo, string? reason)> Remember(long userId,
        short collectionId,
        long scheduleUserId,
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
        
        var closestRepeatDate = DateTime.MaxValue;
        var closestPhaseIndex = -1;
        PhaseEntity? closestPhaseInfo = null;

        await using var transaction = await db.Database.BeginTransactionAsync();
        var forbidDate = DateTime.UtcNow.Date.AddDays(1);

        foreach (var rememberItem in rememberItems)
        {
            var cardId = rememberItem.CardId;
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

            var currentPhase = schedule.Phases
                .OrderBy(p => p.Id)
                .Skip(queueItem.PhaseIndex)
                .First();

            var phaseRemember = new PhaseRememberEntity(
                schedule.ParentUserId,
                schedule.Id,
                currentPhase.Id,
                userId,
                weight);

            db.Entry(phaseRemember).State = EntityState.Added;
            db.Entry(queueItem).State = EntityState.Deleted;

            var (nextPhaseIndex, nextPhase) = GetNextPhaseIndex(schedule, remember);

            if (nextPhase == null)
                continue;

            var nextRepeatDate = now.AddSeconds(nextPhase.SecondsFromLastPhase);

            if (nextRepeatDate < closestRepeatDate)
            {
                closestRepeatDate = nextRepeatDate;
                closestPhaseInfo = nextPhase;
                closestPhaseIndex = nextPhaseIndex;
            }

            var newQueueItem = new CardRepeatQueueEntity(
                schedule.ParentUserId,
                schedule.Id,
                userId,
                collectionId,
                cardId,
                (short)nextPhaseIndex,
                nextRepeatDate);

            db.Entry(newQueueItem).State = EntityState.Added;
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

    private (int nextPhaseIndex, PhaseEntity? nextPhase) GetNextPhaseIndex(RepeatsScheduleEntity schedule, RememberEntity remember)
    {
        var orderedPhases = schedule.Phases.OrderBy(p => p.Id).ToList();

        var currentPhaseIndex = remember.PhaseIndex;
        var nextPhaseIndex = currentPhaseIndex + 1;

        var nextPhase = nextPhaseIndex < orderedPhases.Count 
            ? orderedPhases[currentPhaseIndex + 1] 
            : null;

        if (nextPhase == null)
            return (-1, null);

        var nextPhaseIsRepeatPhase = nextPhase.SecondsFromLastPhase < 10;

        if (nextPhaseIsRepeatPhase)
        {
            if (remember.Weight >= 0.49f)
            {
                nextPhaseIndex++;
                nextPhase = nextPhaseIndex < orderedPhases.Count ? orderedPhases[nextPhaseIndex] : null;
            }

            return (nextPhaseIndex, nextPhase);
        }

        var currentPhase = orderedPhases[currentPhaseIndex];
        var isCurrentPhaseRepeatPhase = currentPhase.SecondsFromLastPhase < 10;

        if (isCurrentPhaseRepeatPhase && currentPhaseIndex > 0)
        {
            var previousRemembers = db.Remembers
                .Where(r => r.ParentUserId == remember.ParentUserId
                            && r.ParentCollectionId == remember.ParentCollectionId
                            && r.ParentCardId == remember.ParentCardId
                            && r.ParentRepeatsScheduleUserId == remember.ParentRepeatsScheduleUserId
                            && r.ParentRepeatsScheduleId == remember.ParentRepeatsScheduleId
                            && r.PhaseIndex == remember.PhaseIndex - 1)
                .ToList();

            var previousRemember = previousRemembers.MaxBy(r => r.Id);

            if (previousRemember != null)
            {
                currentPhaseIndex = previousRemember.PhaseIndex;
                currentPhase = orderedPhases[currentPhaseIndex];
                remember = previousRemember;
            }
        }

        if (remember.Weight >= 0.49f)
        {
            return (nextPhaseIndex, nextPhase);
        }

        switch (schedule.ForgottenBehavior)
        {
            case ForgottenBehavior.MoveToNextStep:
                return (nextPhaseIndex, nextPhase);
            case ForgottenBehavior.StayOnCurrentStep:
                return (currentPhaseIndex, currentPhase);
            case ForgottenBehavior.StartFromFirstStep:
                return (0, orderedPhases[0]);
            case ForgottenBehavior.MoveToPreviousStep:
            {
                if (currentPhaseIndex == 0)
                    return (0, orderedPhases[0]);

                var previousPhaseIndex = currentPhaseIndex - 1;
                var previousPhase = orderedPhases[previousPhaseIndex];

                var isPreviousPhaseRepeatPhase = previousPhase.SecondsFromLastPhase < 10;

                if (isPreviousPhaseRepeatPhase && previousPhaseIndex > 0)
                {
                    previousPhase = orderedPhases[previousPhaseIndex - 1];
                    previousPhaseIndex -= 1;
                }
                
                return (previousPhaseIndex, previousPhase);
            }
            default: throw new InvalidOperationException("Unknown forgotten behaviour " + schedule.ForgottenBehavior);
        };
    }

    public class CreateOrPatchCard : ICreateOrPatchCard
    {
        public string FrontSideText { get; }
        public string PromptText { get; }
        public string BackSideText { get; }
        public string? Description { get; }
        public List<string>? Examples { get; }
        public long ParentUserId { get; }
        public short ParentCollectionId { get; }

        public CreateOrPatchCard(
            long parentUserId,
            short parentCollectionId,
            string frontSideText,
            string promptText,
            string backSideText,
            string? description,
            List<string>? examples)
        {
            ParentUserId = parentUserId;
            ParentCollectionId = parentCollectionId;
            PromptText = promptText;
            FrontSideText = TextMaster.RemoveWhiteSpaces(frontSideText, true);
            BackSideText = TextMaster.RemoveWhiteSpaces(backSideText, true);
            Description = TextMaster.RemoveWhiteSpaces(description);
            Examples = examples?
                .Select(e => TextMaster.RemoveWhiteSpaces(e))
                .Where(e => !string.IsNullOrEmpty(e))
                .ToList();
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