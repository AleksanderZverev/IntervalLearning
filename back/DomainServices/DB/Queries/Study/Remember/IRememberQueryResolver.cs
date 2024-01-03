using Domain.Card;
using Domain.Card.ValueObjects;
using Domain.Collection.ValueObjects;
using Domain.Schedule.ValueObjects;
using Domain.User.ValueObjects;

namespace DomainServices.DB.Queries.Study.Remember;

public interface IRememberQueryResolver
{
    Task<List<global::Domain.Schedule.Entities.Remember.Remember>> GetRangeForCollection(
        UserId userId,
        CollectionId collectionId,
        UserId scheduleUserId,
        ScheduleId scheduleId);
    
    Task<List<global::Domain.Schedule.Entities.Remember.Remember>> GetRangeForCards(
        UserId userId,
        CollectionId collectionId,
        UserId scheduleUserId,
        ScheduleId scheduleId,
        List<CardId> cardsIds);

    Task<List<Card>> GetCanStartCards(UserId userId, UserId scheduleUserId, ScheduleId scheduleId);
}