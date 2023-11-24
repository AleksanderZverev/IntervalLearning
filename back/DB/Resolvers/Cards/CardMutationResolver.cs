using Application.Common.Interfaces.Domain.Cards;
using DB.Configurations.Study;
using Domain.Card;
using Domain.Card.ValueObjects;
using Domain.Collection.ValueObjects;
using Domain.User.ValueObjects;
using FluentResults;
using Infrastructure.Errors;

namespace DB.Resolvers.Cards;

public class CardMutationResolver : ICardsMutationResolver
{
    private readonly ApplicationContext db;

    public CardMutationResolver(ApplicationContext db)
    {
        this.db = db;
    }

    public Result<Card> Add(Card entity)
    {
        db.Cards.Add(entity);
        return db.SoftSaveChanges()
            ? entity
            : new InternalError();
    }

    public Result<Card> Update(Card entity)
    {
        db.Cards.Update(entity);
        return db.SoftSaveChanges()
            ? entity
            : new InternalError();
    }

    public Result<Card> Delete(Card entity)
    {
        db.Cards.Remove(entity);
        return db.SoftSaveChanges()
            ? entity
            : new InternalError();
    }

    public Result<CardId> GetUniqueId(UserId userId, CollectionId collectionId)
    {
        var sequenceName = CardConfiguration.GetSequenceName(userId, collectionId);
        db.EnsureSequenceCreated(sequenceName);
        var nextCardId = db.GetSequenceNextValue16(sequenceName);
        return CardId.Create(nextCardId);
    }
}