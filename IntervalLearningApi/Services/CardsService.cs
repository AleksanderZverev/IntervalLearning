using DB;
using DB.Models;
using Microsoft.EntityFrameworkCore;

namespace IntervalLearningApi.Services;

public class CardsService
{
    private readonly ApplicationContext db;

    public CardsService(ApplicationContext db)
    {
        this.db = db;
    }

    public (CardEntity? card, string? error) Create(
        long userId,
        short collectionId,
        string frontText,
        string backText,
        short scheduleId,
        string? description = null,
        List<string>? examples = null)
    {
        var card = new CardEntity(
            userId,
            collectionId,
            frontText,
            backText,
            scheduleId,
            description,
            examples
        );

        try
        {
            db.Cards.Add(card);
            db.SaveChanges();
            return (card, null);
        }
        catch
        {
            return (null, "Unknown error");
        }
    }

    public (bool ok, string? reason) Start(long userId, short collectionId, short cardId) =>
        ChangeState(userId, collectionId, cardId, false);

    public (bool ok, string? reason) Finish(long userId, short collectionId, short cardId) =>
        ChangeState(userId, collectionId, cardId, true);

    public (bool ok, string? reason) SetNotStarted(long userId, short collectionId, short cardId) =>
        ChangeState(userId, collectionId, cardId, null);

    private (bool ok, string? reason) ChangeState(long userId, short collectionId, short cardId, bool? isFinished)
    {
        var cardEntity = db.Cards.Find(userId, collectionId, cardId);

        if (cardEntity == null)
            return (false, "Not found");

        if (cardEntity.IsFinished == isFinished)
            return (true, null);

        cardEntity.IsFinished = isFinished;
        db.SaveChanges();

        return (true, null);
    }

    public (bool ok, string? reason) Remember(
        long userId,
        short collectionId,
        short cardId,
        float weight,
        byte phaseStep,
        int passedSecondsFromLastStem)
    {
        //TODO: use NoTracking??
        var remembers = db.Remembers
            .Where(r => r.ParentUserId == userId &&
                        r.ParentCollectionId == collectionId &&
                        r.ParentCardId == cardId)
            .AsNoTracking()
            .ToList();

        if (remembers.Any(r => r.PhaseStep >= phaseStep))
            return (false, "Conflict");

        var remember = new RememberEntity(
            userId, collectionId, cardId, weight, phaseStep, passedSecondsFromLastStem);

        db.Remembers.Add(remember);
        db.SaveChanges();

        return (true, null);
    }
}