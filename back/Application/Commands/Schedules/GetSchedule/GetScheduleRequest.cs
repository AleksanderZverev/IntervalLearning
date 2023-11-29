using DB.Models.ValueObjects;
using Domain.User.ValueObjects;

namespace Application.Commands.Schedules.GetSchedule;

public record GetScheduleRequest(
    UserId UserId,
    ScheduleId ScheduleId
);