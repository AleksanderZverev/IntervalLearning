using Domain.Card.ValueObjects;
using Domain.Collection.ValueObjects;
using Domain.Queue;
using Domain.Schedule.ValueObjects;
using Domain.User.ValueObjects;

namespace DomainServices.DB.Queries.Study.Queue;

public interface IRepeatingQueueResolver
{
    Task<List<CardRepeatQueue>> GetAll(UserId userId);
    
    Task<List<CardRepeatQueue>> GetAllBySchedule(UserId userId, ComplexScheduleId scheduleId);
    
    Task<(List<CardRepeatQueue> QueuedCardInfos, int TotalCards)> GetAllTillDate(
        int page,
        int cout,
        UserId userId,
        CollectionId collectionId,
        UserId scheduleUserId,
        ScheduleId scheduleId,
        DateTime date);

    Task<List<CardRepeatQueue>> GetForCards(
        UserId userId,
        CollectionId collectionId,
        UserId scheduleUserId,
        ScheduleId scheduleId,
        short phaseIndex,
        List<CardId> cardIds);

    Task<List<CardRepeatQueue>> GetByRange(
        UserId userId,
        UserId scheduleUserId,
        ScheduleId scheduleId,
        DateTime from,
        DateTime to);

    Task<List<CardRepeatQueue>> GetAllForCard(
        UserId userId,
        CollectionId collectionId,
        CardId cardId,
        UserId scheduleUserId,
        ScheduleId scheduleId);
}