using Domain.User.ValueObjects;

namespace Application.Commands.Schedules.GetAvailableSchedules;

public record GetAvailableSchedulesRequest(
    UserId UserId
);