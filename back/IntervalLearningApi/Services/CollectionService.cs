using System.Diagnostics;
using DB;
using DB.Models;
using Infrastructure;
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

    public async Task<Dictionary<DateTime, List<RepeatingPhase>>> GetRepeatCollections(long userId)
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
            var phase = schedule.Phases.OrderBy(p => p.Id).Skip(queueItem.PhaseIndex).First();

            if (!result.ContainsKey(date))
            {
                result.Add(date, new List<RepeatingPhase>());
            }

            var repeatingPhasesList = result[date];

            var repeatingPhase = repeatingPhasesList.SingleOrDefault(r =>
                r.ScheduleUserId == queueItem.ParentRepeatsScheduleUserId
                && r.ScheduleId == queueItem.ParentRepeatsScheduleId
                && r.PhaseIndex == queueItem.PhaseIndex);

            if (repeatingPhase == null)
            {
                repeatingPhase = new RepeatingPhase(
                    queueItem.ParentRepeatsScheduleUserId,
                    queueItem.ParentRepeatsScheduleId,
                    queueItem.PhaseIndex,
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

    public async Task<(int totalCollections, List<CollectionEntity> canStartCollections)> GetCanStart(
        long userId,
        long scheduleUserId,
        short scheduleId,
        int page = 1,
        int count = 30)
    {
        //TODO: can be slow.

        var canStartCards = await db.Cards
            .Where(c => c.ParentUserId == userId
                        && !db.Remembers.Any(r =>
                            r.ParentUserId == userId
                            && r.ParentCollectionId == c.ParentCollectionId
                            && r.ParentCardId == c.Id
                            && r.ParentRepeatsScheduleUserId == scheduleUserId
                            && r.ParentRepeatsScheduleId == scheduleId))
            .ToListAsync();
        
        var skip = (page - 1) * count;

        var canStartAllCollectionsIds = canStartCards
            .Select(c => c.ParentCollectionId)
            .Distinct()
            .ToList();

        var totalCollections = canStartAllCollectionsIds.Count;

        var collectionIdsToStart = canStartAllCollectionsIds.Skip(skip).Take(count).ToList();

        var canStartCollections = await db.Collections
            .Where(c => c.ParentUserId == userId && collectionIdsToStart.Contains(c.Id))
            .ToListAsync();

        var collectionToCardsCount = canStartCollections
            .GroupBy(c => c.Id)
            .ToDictionary(c => c.Key, c => canStartCards.Count(card => card.ParentCollectionId == c.Key));

        foreach (var collection in canStartCollections)
        {
            var notStartedCards = collectionToCardsCount[collection.Id];
            collection.NotStartedCardsCount = (short)notStartedCards;
        }

        return (totalCollections, canStartCollections);
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
            Title = TextMaster.RemoveWhitespaces(title, true);
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
        public short ScheduleId { get;  }
        public short PhaseIndex { get;  }
        public uint SecondsFromLastPhase { get; }
        public string? Description { get; }

        public List<RepeatingCollection> RepeatingCollections { get; set; } = new();

        public RepeatingPhase(
            long scheduleUserId,
            short scheduleId,
            short phaseIndex,
            uint secondsFromLastPhase,
            string? description)
        {
            ScheduleUserId = scheduleUserId;
            ScheduleId = scheduleId;
            PhaseIndex = phaseIndex;
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