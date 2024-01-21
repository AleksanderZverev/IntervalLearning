using DB.Configurations.Study;
using DB.Repository;
using Domain.Card;
using Domain.Card.ValueObjects;
using Domain.Collection.ValueObjects;
using Domain.User.ValueObjects;
using DomainServices.DB.Repositories;
using DomainServices.DB.Repositories.Study.Cards;
using FluentResults;

namespace DB.Resolvers.Cards;

internal class CardsRepository : BaseRepository<Card>, IRepository<Card, CardId, CardIdParams>
{
    public CardsRepository(ApplicationContext db) : base(db)
    {
    }
    
    private static string GetSequenceName(UserId userId, CollectionId collectionId)
        => $"cards_for_user_{userId.Value}_of_collection_{collectionId.Value}"; 

    public override Card Add(Card entity)
    {
        var result = db.Cards.Add(entity).Entity;
    
        if (entity.Remembers is { Count: > 0 })
        {
            db.Remembers.AddRange(entity.Remembers);
        }

        return result;
    }

    public override Card Update(Card entity)
    {
        var result = db.Cards.Update(entity).Entity;
    
        if (entity.Remembers is { Count: > 0 })
        {
            db.Remembers.UpdateRange(entity.Remembers);
        }

        return result;
    }

    public override Card Delete(Card entity)
    {
        var result = db.Cards.Remove(entity).Entity;
        
        if (entity.Remembers is { Count: > 0 })
        {
            db.Remembers.RemoveRange(entity.Remembers);
        }

        return result;
    }

    public Result<CardId> GetUniqueId(CardIdParams param)
    {
        var sequenceName = GetSequenceName(param.UserId, param.CollectionId);
        const int cardsStartId = 5000;
        db.EnsureSequenceCreated(sequenceName, cardsStartId);
        var nextCardId = db.GetSequenceNextValue16(sequenceName, cardsStartId);
        return CardId.Create(nextCardId);
    }
}