using Domain.Card.ValueObjects;
using Domain.Collection.ValueObjects;
using Domain.Schedule.ValueObjects;
using Domain.User.ValueObjects;

namespace Application.Commands.Cards.PostponeRepeatingCard;

public record PostponeRepeatingCardCommandRequest(
    UserId UserId,
    CollectionId CollectionId,
    CardId CardId,
    UserId ScheduleUserId,
    ScheduleId ScheduleId,
    int PostponeDays,
    bool AllowPostponeFutureRepetitions
);