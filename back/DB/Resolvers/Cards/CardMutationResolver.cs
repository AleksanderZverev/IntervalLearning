using Application.Common.Interfaces.Domain.Cards;
using DB.Configurations.Study;
using Domain.Card;
using Domain.Card.ValueObjects;
using Domain.Collection.ValueObjects;
using Domain.User.ValueObjects;
using FluentResults;
using Infrastructure.Errors;

namespace DB.Resolvers.Cards;

public class CardMutationResolver : BaseMutationResolver<Card>, ICardsMutationResolver
{
    public CardMutationResolver(ApplicationContext db) : base(db)
    {
    }

    protected override void MarkAdded(Card entity)
    {
        db.Cards.Add(entity);
    
        if (entity.Remembers is { Count: > 0 })
        {
            db.Remembers.AddRange(entity.Remembers);
        }
    }

    protected override void MarkUpdated(Card entity)
    {
        db.Cards.Update(entity);
    
        if (entity.Remembers is { Count: > 0 })
        {
            db.Remembers.UpdateRange(entity.Remembers);
        }
    }

    protected override void MarkRemoved(Card entity)
    {
        db.Cards.Remove(entity);
        
        if (entity.Remembers is { Count: > 0 })
        {
            db.Remembers.RemoveRange(entity.Remembers);
        }
    }

    public Result<CardId> GetUniqueId(UserId userId, CollectionId collectionId)
    {
        var sequenceName = CardConfiguration.GetSequenceName(userId, collectionId);
        db.EnsureSequenceCreated(sequenceName);
        var nextCardId = db.GetSequenceNextValue16(sequenceName);
        return CardId.Create(nextCardId);
    }
}