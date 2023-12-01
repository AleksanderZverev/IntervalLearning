using Application.Commands.Cards.SearchCards;
using Domain.Card;
using Domain.Card.ValueObjects;
using Domain.Collection.ValueObjects;
using Domain.User.ValueObjects;

namespace Application.Common.Interfaces.Domain.Cards;

public interface ICardsQueryResolver
{
    public Task<Card?> Find(UserId userId, CollectionId collectionId, CardId cardId);
    public Task<List<Card>> GetAll(UserId userId, CollectionId collectionId);
    Task<List<Card>> GetRange(UserId userId, CollectionId collectionId, List<CardId> cardsIds);
    Task<List<Card>> GetExceptRange(UserId userId, CollectionId collectionId, List<CardId> excludeCardIds);

    Task<List<Card>> Search(
        UserId userId,
        CollectionId collectionId,
        string searchValue,
        SearchFieldType fieldType,
        int page,
        int count);

    Task<List<Card>> GetRangeFromCollections(UserId userId, List<CollectionId> collectionIds);
}
