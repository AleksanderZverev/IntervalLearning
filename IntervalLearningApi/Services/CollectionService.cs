using DB;
using DB.Models;
using Microsoft.EntityFrameworkCore;

namespace IntervalLearningApi.Services;

public class CollectionService
{
    private readonly ApplicationContext db;
    private readonly CardsService cardsService;

    public CollectionService(ApplicationContext db, CardsService cardsService)
    {
        this.db = db;
        this.cardsService = cardsService;
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

    public (CardEntity? card, string? error) AddCard(
        long userId,
        short collectionId,
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

        var (card, error) = cardsService.Create(
            userId, collectionId, frontText, backText, scheduleUserId, scheduleId, description, examples);

        if (error != null)
        {
            db.Database.RollbackTransaction();
            return (card, error);
        }

        collection.CardsCount++;
        db.SaveChanges();

        db.Database.CommitTransaction();

        return (card, null);
    }
}