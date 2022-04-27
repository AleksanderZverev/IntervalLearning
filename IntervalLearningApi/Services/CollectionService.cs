using System.Collections.Generic;
using DB;
using DB.Models;
using Microsoft.EntityFrameworkCore;
using NodaTime;

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

    public async Task<Dictionary<DateTime, List<QueueCollection>>> GetQueueCollections(long userId)
    {
        var queueItems = await db.Queue
            .Where(q => q.ParentUserId == userId)
            .ToListAsync();

        var collectionIds = queueItems
            .Select(q => q.ParentCollectionId)
            .Distinct()
            .ToList();

        var collectionIdToCollection = await db.Collections
            .Where(c => c.ParentUserId == userId && collectionIds.Contains(c.Id))
            .ToDictionaryAsync(c => c.Id);

        var result = new Dictionary<DateTime, List<QueueCollection>>();

        foreach (var queueItem in queueItems)
        {
            var date = queueItem.Date.Date;//queueItem.Date.ToDateTimeUtc().Date;

            if (!result.ContainsKey(date))
                result.Add(date, new List<QueueCollection>());

            var collection = collectionIdToCollection[queueItem.ParentCollectionId];

            var collectionsList = result[date];
            var queueCollection = collectionsList.SingleOrDefault(q => q.Collection.Id == queueItem.ParentCollectionId);

            if (queueCollection == null)
            {
                queueCollection = new QueueCollection(collection);
                collectionsList.Add(queueCollection);
            }

            queueCollection.CardsToRepeatCount++;
        }

        return result;
    }

    public async Task<(List<CollectionEntity> started, List<CollectionEntity> notStarted)> GetNotFinished(long userId, int page = 1, int count = 30)
    {
        var metadata = metadataService.GetMetadata(userId);

        var totalCollections = page * count;
        var skip = (page - 1) * count;

        //if (skip > (metadata.NotStartedCollections + metadata.StartedCollections))
        //    return (new List<CollectionEntity>(0), new List<CollectionEntity>(0));

        var startedCollections = await db.Collections
            .Where(c => c.NotStartedCards != 0 && c.StartedCards > 0)
            .ToListAsync();

        if (totalCollections <= startedCollections.Count)
        {
            var started = startedCollections
                .Skip(skip)
                .Take(count)
                .ToList();

            return (started, new List<CollectionEntity>(0));
        }

        var notStartedCollectionsToTake = totalCollections - startedCollections.Count;

        if (notStartedCollectionsToTake <= count)
        {
            var started = startedCollections
                .Skip(skip)
                .Take(count)
                .ToList();

            var notStarted = await db.Collections
                .Where(c => c.NotStartedCards > 0 && c.StartedCards == 0)
                .Take(notStartedCollectionsToTake)
                .ToListAsync();

            return (started, notStarted);
        }
        
        var newPage = (int) Math.Ceiling((double) metadata.NotStartedCollections / count);
        var newSkip = (skip - metadata.NotStartedCollections) + ((newPage - 1) * count);

        var notStartedCollections = db.Collections
            .Where(c => c.NotStartedCards == 0)
            .Skip(newSkip)
            .Take(count)
            .ToList();

        return (new List<CollectionEntity>(0), notStartedCollections);
    }

    public (CollectionEntity? collection, string? error) Create(
        long userId, 
        long repeatsScheduleUserId,
        short repeatsScheduleId, 
        short themeId, 
        string title, 
        bool isDefaultBackSide)
    {
        var collection = new CollectionEntity(
            userId,
            repeatsScheduleUserId,
            repeatsScheduleId,
            themeId,
            title,
            isDefaultBackSide
        );

        try
        {
            db.Entry(collection).State = EntityState.Added;
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
        long scheduleUserId,
        short scheduleId,
        string? description,
        List<string>? examples)
    {
        var collection = db.Collections.Find(userId, collectionId);

        if (collection == null)
            return (null, "Collection not found");

        db.Database.BeginTransaction();

        var (card, error, isCreated) = cardsService.CreateOrEdit(
            userId, collectionId, cardId, frontText, backText, scheduleUserId, scheduleId, description, examples);

        if (error != null)
        {
            db.Database.RollbackTransaction();
            return (card, error);
        }

        if (isCreated)
        {
            collection.CardsCount++;
            collection.NotStartedCards++;
        }

        db.SaveChanges();

        db.Database.CommitTransaction();

        return (card, null);
    }

    public class QueueCollection
    {
        public CollectionEntity Collection { get; }

        public int CardsToRepeatCount { get; set; }

        public QueueCollection(CollectionEntity collection)
        {
            Collection = collection;
        }
    }
}