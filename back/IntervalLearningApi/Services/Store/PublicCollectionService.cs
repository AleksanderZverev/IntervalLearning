using DB;
using DB.Models.Store;
using Microsoft.EntityFrameworkCore;

namespace IntervalLearningApi.Services.Store;

public class PublicCollectionService
{
    private readonly ApplicationContext db;
    private readonly PublicCardsService publicCardsService;

    public PublicCollectionService(ApplicationContext db, PublicCardsService publicCardsService)
    {
        this.db = db;
        this.publicCardsService = publicCardsService;
    }

    public (PublicCollectionEntity? collection, string? error) Create(CreatePublicCollection item)
    {
        var collection = db.CreateByProperties<PublicCollectionEntity>(item);
        var isCreated = db.SoftSaveChanges();
        return isCreated ? (collection, null) : (null, "Unknown error");
    }

    public (PublicCollectionEntity? collection, string? error) Edit(PatchPublicCollection item, long userId, short collectionId)
    {
        var collection = db.UpdateByProperties<PublicCollectionEntity>(item, userId, collectionId);
        var isUpdated = db.SoftSaveChanges();
        return isUpdated ? (collection, null) : (null, "Unknown error");
    }

    public (PublicCardEntity? card, string? error) AddCard(CreatePublicCard item, long userId, short collectionId)
    {
        var collection = db.PublicCollections.Find(userId, collectionId);

        if (collection == null)
        {
            return (null, "Collection not found");
        }

        db.Database.BeginTransaction();

        var (card, error) = publicCardsService.Create(item);

        if (card == null)
        {
            db.Database.RollbackTransaction();
            return (null, error);
        }

        collection.CardsCount++;
        db.SaveChanges();

        db.Database.CommitTransaction();
        return (card, null);
    }

    public Task<PublicCollectionEntity?> Find(long publicCollectionUserId, short publicCollectionId)
    {
        return db.PublicCollections.FindAsync(publicCollectionUserId, publicCollectionId).AsTask();
    }
}

public class PublicCardsService
{
    private readonly ILogger<PublicCardsService> logger;
    private readonly IWebHostEnvironment env;
    private readonly ApplicationContext db;
    private readonly UserMetadataService metadataService;

    public PublicCardsService(ILogger<PublicCardsService> logger,
        IWebHostEnvironment env,
        ApplicationContext db,
        UserMetadataService metadataService)
    {
        this.logger = logger;
        this.env = env;
        this.db = db;
        this.metadataService = metadataService;
    }

    public Task<List<PublicCardEntity>> GetAllCards(long userId, short collectionId)
    {
        return db.PublicCards
            .Where(c => c.OwnerUserId == userId && c.PublicCollectionId == collectionId)
            .ToListAsync();
    }

    public Task<List<PublicCardEntity>> GetCards(long userId, short collectionId, int page, int count)
    {
        var toSkip = (page - 1) * count;

        return db.PublicCards
            .Where(c => c.OwnerUserId == userId && c.PublicCollectionId == collectionId)
            .Skip(toSkip)
            .Take(count)
            .ToListAsync();
    }

    public (PublicCardEntity? card, string? error) Create(CreatePublicCard item)
    {
        var card = db.CreateByProperties<PublicCardEntity>(item);
        var isCreated = db.SoftSaveChanges();
        return isCreated ? (card, null) : (null, "Unknown error");
    }

    public (PublicCardEntity? card, string? error) Edit(PatchPublicCard item, long userId, short collectionId, short cardId)
    {
        var card = db.UpdateByProperties<PublicCardEntity>(item, userId, collectionId, cardId);
        var isUpdated = db.SoftSaveChanges();
        return isUpdated ? (card, null) : (null, "Unknown error");
    }
}