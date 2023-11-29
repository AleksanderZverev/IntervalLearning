using DB.Models.ValueObjects;
using Domain.Schedule;
using Domain.User.ValueObjects;

namespace Application.Common.Interfaces.Domain.Study.Schedule;

public interface IScheduleResolver
{
    Task<RepeatsSchedule?> FindAsync(UserId userId, ScheduleId scheduleId);
}