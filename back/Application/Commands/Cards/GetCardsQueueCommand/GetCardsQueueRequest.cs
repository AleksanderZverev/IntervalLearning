using Domain.Collection.ValueObjects;
using Domain.Schedule.ValueObjects;
using Domain.User.ValueObjects;

namespace Application.Commands.Cards.GetCardsQueueCommand;

public record GetCardsQueueRequest(
    UserId UserId,
    CollectionId CollectionId,
    UserId ScheduleUserId,
    ScheduleId ScheduleId,
    short PhaseIndex,
    DateTime Date,
    bool CheckRepeatableDate);