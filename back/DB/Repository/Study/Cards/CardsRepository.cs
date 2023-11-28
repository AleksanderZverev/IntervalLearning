using Application.Common.Interfaces.DB.Repositories;
using Application.Common.Interfaces.DB.Repositories.Cards;
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

    public Result<CardId> GetUniqueId(CardIdParams param)
    {
        var sequenceName = CardConfiguration.GetSequenceName(param.UserId, param.CollectionId);
        db.EnsureSequenceCreated(sequenceName);
        var nextCardId = db.GetSequenceNextValue16(sequenceName);
        return CardId.Create(nextCardId);
    }
}