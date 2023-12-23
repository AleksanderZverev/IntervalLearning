using Application.Common.Interfaces.DB.Repositories;
using Application.Common.Interfaces.DB.Repositories.Study.Cards;
using DB.Configurations.Study;
using DB.Repository;
using Domain.Card;
using Domain.Card.ValueObjects;
using FluentResults;

namespace DB.Resolvers.Cards;

internal class CardsRepository : BaseRepository<Card>, IRepository<Card, CardId, CardIdParams>
{
    public CardsRepository(ApplicationContext db) : base(db)
    {
    }

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
        var sequenceName = CardConfiguration.GetSequenceName(param.UserId, param.CollectionId);
        db.EnsureSequenceCreated(sequenceName);
        var nextCardId = db.GetSequenceNextValue16(sequenceName);
        return CardId.Create(nextCardId);
    }
}