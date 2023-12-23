using Domain.Schedule.ValueObjects;
using Domain.User.ValueObjects;

namespace Application.Commands.Collections.GetRepeatCollections;

public record RepeatingPhase(
    UserId ScheduleUserId,
    ScheduleId ScheduleId,
    short PhaseIndex,
    uint SecondsFromLastPhase,
    string? Description,
    List<RepeatingCollection> RepeatingCollections
);