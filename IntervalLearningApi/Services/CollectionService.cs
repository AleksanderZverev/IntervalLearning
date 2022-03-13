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

    public List<CollectionEntity> GetAllByUserId(long userId)
    {
        var collections = db.Collections
            .Where(c => c.ParentUserId == userId)
            .Include(c => c.Cards)
            .ThenInclude(c => c.Remembers)
            .AsNoTracking()
            .ToList();

        return collections;
    }

    public (CollectionEntity? collection, string? error) Create(
        long userId, 
        short repeatsScheduleId, 
        short themeId, 
        string title, 
        bool isDefaultBackSide)
    {
        var collection = new CollectionEntity(
            userId,
            repeatsScheduleId,
            themeId,
            title,
            isDefaultBackSide
        );

        try
        {
            db.Collections.Add(collection);
            db.SaveChanges();
            return (collection, null);
        }
        catch
        {
            return (null, "Unknown error");
        }
    }

    public void AddCard(
        long userId,
        short collectionId,
        string frontText,
        string backText,
        short scheduleId,
        string description = null,
        List<string> examples = null)
    {
        var collection = db.Collections.Find(userId, collectionId);

        if (collection == null)
            return;

        var card = cardsService.Create(
            userId, collectionId, frontText, backText, scheduleId, description, examples);
    }
}