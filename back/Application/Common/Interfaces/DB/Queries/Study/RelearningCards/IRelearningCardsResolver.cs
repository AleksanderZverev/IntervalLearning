using Domain.Card.ValueObjects;
using Domain.Collection;
using Domain.Collection.ValueObjects;
using Domain.RelearningCard;
using Domain.User.ValueObjects;

namespace Application.Common.Interfaces.DB.Queries.Study.RelearningCards;

public interface IRelearningCardsResolver
{
    Task<List<RelearningCard>> GetAll(UserId userId);
    Task<List<RelearningCard>> GetAllFor(UserId userId, CollectionId collectionId);
    Task<RelearningCard?> Find(UserId userId, CollectionId collectionId, CardId cardId);
}