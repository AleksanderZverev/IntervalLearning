using Domain.Collection.ValueObjects;
using Domain.Schedule.ValueObjects;
using Domain.User.ValueObjects;

namespace Application.Commands.Cards.GetCardsQueueCommand;

public record GetCardsQueueRequest(
    int Page,
    int CardsCountByPage,
    UserId UserId,
    CollectionId CollectionId,
    UserId ScheduleUserId,
    ScheduleId ScheduleId,
    bool IsRepeatingMode,
    DateTime Date,
    bool CheckRepeatableDate,
    DateTimeOffset UserCurrentDateTime);