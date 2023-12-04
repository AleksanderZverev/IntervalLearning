using Domain.Schedule.ValueObjects;
using Domain.User.ValueObjects;

namespace Application.Commands.Collections.GetCanStartCollections;

public record GetCanStartCollectionsCommandRequest(
    UserId UserId,
    UserId ScheduleUserId,
    ScheduleId ScheduleId,
    int Page = 1,
    int Count = 30
);