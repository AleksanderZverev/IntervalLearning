using Domain.Collection.ValueObjects;
using Domain.Schedule.ValueObjects;
using Domain.User.ValueObjects;

namespace Application.Commands.Cards.GetNotStartedCardsCommand;

public record GetNotStartedCardsRequest(
    UserId ScheduleUserId,
    ScheduleId ScheduleId,
    UserId UserId,
    CollectionId CollectionId,
    int Count
);