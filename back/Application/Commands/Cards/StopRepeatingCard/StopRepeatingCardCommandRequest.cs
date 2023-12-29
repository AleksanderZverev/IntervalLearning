using Domain.Card.ValueObjects;
using Domain.Collection.ValueObjects;
using Domain.Schedule.ValueObjects;
using Domain.User.ValueObjects;

namespace Application.Commands.Cards.StopRepeatingCard;

public record StopRepeatingCardCommandRequest(
    UserId UserId,
    CollectionId CollectionId,
    CardId CardId,
    UserId ScheduleUserId,
    ScheduleId ScheduleId);