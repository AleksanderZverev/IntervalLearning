using System.Diagnostics;
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

    public async Task<(List<CardEntity>? cards, string? error)> GetNotStartedCards(
        long scheduleUserId,
        short scheduleId, 
        long userId, 
        short collectionId)
    {
        var schedule = db.RepeatsSchedules.Find(scheduleUserId, scheduleId);

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
            .Take(schedule.CardsCountPerPhase)
            .ToListAsync();

        return (canStartCards, null);
    }

    public async Task<List<CardEntity>> GetCardsQueue(
        long userId, 
        short collectionId,
        long scheduleUserId,
        short scheduleId,
        short phaseIndex)
    {
        var queueItems = await db.Queue
            .Where(c => c.ParentUserId == userId 
                        && c.ParentCollectionId == collectionId
                        && c.ParentRepeatsScheduleUserId == scheduleUserId
                        && c.ParentRepeatsScheduleId == scheduleId
                        && c.PhaseIndex == phaseIndex
                        && c.Date.Date <= DateTime.UtcNow.Date)
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
        PhaseEntity closestPhaseInfo = null;
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
        var cardIds = rememberItems.Select(c => c.CardId).ToList();

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
        PhaseEntity closestPhaseInfo = null;

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

            var currentPhase = schedule.Phases
                .OrderBy(p => p.Id)
                .Skip(queueItem.PhaseIndex)
                .First();

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

            var phaseRemember = new PhaseRememberEntity(
                schedule.ParentUserId,
                schedule.Id,
                currentPhase.Id,
                userId,
                weight);

            db.Entry(phaseRemember).State = EntityState.Added;

            var nextPhaseIndex = remember.PhaseIndex + 1;
            var nextPhase = schedule.Phases.Skip(nextPhaseIndex).FirstOrDefault();

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

            db.Entry(queueItem).State = EntityState.Deleted;
            db.Entry(newQueueItem).State = EntityState.Added;
            //db.Entry(collection).State = EntityState.Modified;
        }


        try
        {
            db.SaveChanges();
            return (new NextRepeatInfo(
                closestRepeatDate == DateTime.MaxValue ? null : closestRepeatDate,
                closestPhaseInfo,
                closestPhaseIndex), null);
        }
        catch
        {
            return (null, "unknown error");
        }
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
            FrontSideText = TextMaster.RemoveWhitespaces(frontSideText, true);
            BackSideText = TextMaster.RemoveWhitespaces(backSideText, true);
            Description = string.IsNullOrEmpty(description) ? description : TextMaster.RemoveWhitespaces(description);
            Examples = examples?
                .Select(e => TextMaster.RemoveWhitespaces(e))
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