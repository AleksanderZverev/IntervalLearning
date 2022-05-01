using DB;
using DB.Models;
using Microsoft.EntityFrameworkCore;

namespace IntervalLearningApi.Services;

public class CollectionService
{
    private readonly ApplicationContext db;
    private readonly CardsService cardsService;
    private readonly UserMetadataService metadataService;

    public CollectionService(ApplicationContext db, CardsService cardsService, UserMetadataService metadataService)
    {
        this.db = db;
        this.cardsService = cardsService;
        this.metadataService = metadataService;
    }

    public Task<CollectionEntity?> Find(long userId, short collectionId)
    {
        return db.Collections.FindAsync(userId, collectionId).AsTask();
    }

    public Task<List<CollectionEntity>> GetAllByUserId(long userId)
    {
        var collections = db.Collections
            .Where(c => c.ParentUserId == userId)
            .ToListAsync();

        return collections;
    }

    public async Task<Dictionary<DateTime, List<RepeatingPhase>>> GetQueueCollections(long userId)
    {
        var queueItems = await db.Queue
            .Where(q => q.ParentUserId == userId)
            .Include(q => q.ParentRepeatsSchedule).ThenInclude(q => q.Phases)
            .AsSplitQuery()
            .ToListAsync();

        var collectionIds = queueItems
            .Select(q => q.ParentCollectionId)
            .Distinct()
            .ToList();

        var collectionIdToCollection = await db.Collections
            .Where(c => c.ParentUserId == userId && collectionIds.Contains(c.Id))
            .ToDictionaryAsync(c => c.Id);

        var result = new Dictionary<DateTime, List<RepeatingPhase>>();

        foreach (var queueItem in queueItems)
        {
            var date = queueItem.Date.Date;
            var schedule = queueItem.ParentRepeatsSchedule;
            var phase = schedule.Phases.Single(p => p.Id == queueItem.PhaseId);

            if (!result.ContainsKey(date))
            {
                result.Add(date, new List<RepeatingPhase>());
            }

            var repeatingPhasesList = result[date];

            var repeatingPhase = repeatingPhasesList.SingleOrDefault(p =>
                p.ScheduleUserId == queueItem.ParentRepeatsScheduleUserId
                && p.ScheduleUserId == queueItem.ParentRepeatsScheduleId
                && p.PhaseStep == queueItem.PhaseId);

            if (repeatingPhase == null)
            {
                repeatingPhase = new RepeatingPhase(
                    queueItem.ParentRepeatsScheduleUserId,
                    queueItem.ParentRepeatsScheduleId,
                    queueItem.PhaseId,
                    phase.SecondsFromLastPhase,
                    phase.Description);

                repeatingPhasesList.Add(repeatingPhase);
            }

            var collection = collectionIdToCollection[queueItem.ParentCollectionId];

            var repeatingCollection =
                repeatingPhase.RepeatingCollections.SingleOrDefault(q =>
                    q.Collection.Id == queueItem.ParentCollectionId);

            if (repeatingCollection == null)
            {
                repeatingCollection = new RepeatingCollection(collection);
                repeatingPhase.RepeatingCollections.Add(repeatingCollection);
            }

            repeatingCollection.CardsToRepeatCount++;
        }

        return result;
    }

    public async Task<List<CollectionEntity>> GetCanStart(
        long userId,
        long scheduleUserId,
        short scheduleId,
        int page = 1,
        int count = 30)
    {
        //TODO: can be mistakes if started today. Need to use metadata
        var startedCardIds = await db.Remembers
            .Where(r => r.ParentUserId == userId
                        && r.ParentRepeatsScheduleUserId == scheduleUserId
                        && r.ParentRepeatsScheduleId == scheduleId)
            .Select(c => c.ParentCardId)
            .ToListAsync();

        var canStartCards = await db.Cards
            .Where(c => c.ParentUserId == userId && !startedCardIds.Contains(c.Id))
            .ToListAsync();

        var totalCollections = page * count;
        var skip = (page - 1) * count;

        var canStartCollectionsId = canStartCards
            .Select(c => c.ParentCollectionId)
            .Skip(skip)
            .Take(totalCollections)
            .ToList();

        var canStartCollections = await db.Collections
            .Where(c => c.ParentUserId == userId && canStartCollectionsId.Contains(c.Id))
            .ToListAsync();


        var collectionToCardsCount = canStartCards
            .GroupBy(c => c.Id)
            .ToDictionary(c => c.Key, c => c.Count());

        foreach (var collection in canStartCollections)
        {
            var notStartedCards = collectionToCardsCount[collection.Id];
            collection.NotStartedCardsCount = (short)notStartedCards;
        }

        return canStartCollections;
    }

    public class CreateOrPatchCollection : ICreateOrEditModel
    {
        public long ParentUserId { get; }
        public short ThemeId { get; }
        public string Title { get;  }
        public bool IsDefaultBackSide { get; }

        public CreateOrPatchCollection(
            long parentUserId, 
            string title, 
            bool isDefaultBackSide,
            short themeId)
        {
            ParentUserId = parentUserId;
            Title = title;
            IsDefaultBackSide = isDefaultBackSide;
            ThemeId = themeId;
        }
    }

    public (CollectionEntity? collection, string? error) CreateOrEdit(CreateOrPatchCollection item, short? collectionId)
    {
        var collection = collectionId == null
            ? new CollectionEntity()
            : db.Collections.Find(item.ParentUserId, collectionId);

        if (collection == null)
            return (null, "Collection not found");

        var entry = db.Entry(collection);
        entry.CurrentValues.SetValues(item);

        try
        {
            if (collectionId == null)
                entry.State = EntityState.Added;

            db.SaveChanges();
            return (collection, null);
        }
        catch
        {
            return (null, "Unknown error");
        }
    }

    public (CardEntity? card, string? error) CreateOrEditCard(long userId,
        short collectionId,
        short? cardId,
        string frontText,
        string backText,
        string? description,
        List<string>? examples)
    {
        var collection = db.Collections.Find(userId, collectionId);

        if (collection == null)
            return (null, "Collection not found");

        db.Database.BeginTransaction();

        var (card, error, isCreated) = cardsService.CreateOrEdit(
            new CardsService.CreateOrPatchCard(
                userId,
                collectionId,
                frontText,
                backText,
                description,
                examples),
            cardId);

        if (error != null)
        {
            db.Database.RollbackTransaction();
            return (card, error);
        }

        if (isCreated)
        {
            collection.CardsCount++;
        }

        db.SaveChanges();

        db.Database.CommitTransaction();

        return (card, null);
    }

    public class RepeatingPhase
    {
        public long ScheduleUserId { get; }
        public long ScheduleId { get;  }
        public short PhaseStep { get;  }
        public uint SecondsFromLastPhase { get; }
        public string? Description { get; }

        public List<RepeatingCollection> RepeatingCollections { get; set; } = new();

        public RepeatingPhase(
            long scheduleUserId,
            long scheduleId,
            short phaseStep,
            uint secondsFromLastPhase,
            string? description)
        {
            ScheduleUserId = scheduleUserId;
            ScheduleId = scheduleId;
            PhaseStep = phaseStep;
            SecondsFromLastPhase = secondsFromLastPhase;
            Description = description;
        }
    }

    public class RepeatingCollection
    {
        public CollectionEntity Collection { get; }

        public int CardsToRepeatCount { get; set; }

        public RepeatingCollection(CollectionEntity collection)
        {
            Collection = collection;
        }
    }
}