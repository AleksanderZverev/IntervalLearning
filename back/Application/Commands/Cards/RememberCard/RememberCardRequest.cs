using DB.Models.ValueObjects;
using Domain.Collection.ValueObjects;
using Domain.User.ValueObjects;

namespace Application.Commands.Cards.RememberCard;

public record RememberCardRequest(
    UserId UserId,
    CollectionId CollectionId,
    UserId ScheduleUserId,
    ScheduleId ScheduleId,
    short PhaseIndex,
    List<RememberItem> RememberItems,
    bool AllowRepeatingInFuture
);