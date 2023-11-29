using DB.Models.ValueObjects;
using Domain.Card.ValueObjects;
using Domain.Collection.ValueObjects;
using Domain.Queue;
using Domain.User.ValueObjects;

namespace Application.Common.Interfaces.Domain.Study.Queue;

public interface IRepeatingQueueResolver
{
    Task<List<CardRepeatQueue>> GetAll(UserId userId);
    
    Task<List<CardRepeatQueue>> GetByDate(
        UserId userId,
        CollectionId collectionId,
        UserId scheduleUserId,
        ScheduleId scheduleId,
        short phaseIndex,
        DateTime dateTime);

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
}