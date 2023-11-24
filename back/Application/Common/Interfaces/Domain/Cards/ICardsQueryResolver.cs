using Application.Common.Interfaces.DB;
using Domain.Card;
using Domain.Card.ValueObjects;
using Domain.Collection.ValueObjects;
using Domain.User.ValueObjects;

namespace Application.Common.Interfaces.Domain.Cards;

public interface ICardsQueryResolver
{
    public Task<Card?> Find(UserId userId, CollectionId collectionId, CardId cardId);
}