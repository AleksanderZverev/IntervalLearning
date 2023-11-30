using DB.Models.ValueObjects;
using Domain.User.ValueObjects;

namespace Application.Commands.Schedules.UpdateSchedule;

public record UpdateScheduleCommandRequest(
    UserId UserId,
    ScheduleId scheduleId,
    UpdateScheduleProps item
);