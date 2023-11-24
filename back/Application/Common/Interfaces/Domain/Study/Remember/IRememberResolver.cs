using DB.Models.ValueObjects;
using Domain.Card.ValueObjects;
using Domain.Collection.ValueObjects;
using Domain.User.ValueObjects;

namespace Application.Common.Interfaces.Domain.Study.Remember;

public interface IRememberResolver
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
}