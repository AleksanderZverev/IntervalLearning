using DB.Models.ValueObjects;
using Domain.Schedule;
using Domain.User.ValueObjects;

namespace Application.Common.Interfaces.DB.Queries.Study.Schedule;

public interface IScheduleResolver
{
    Task<RepeatsSchedule?> Find(UserId userId, ScheduleId scheduleId);
    Task<List<RepeatsSchedule>> GetUsers(UserId userId);
    Task<List<RepeatsSchedule>> GetRecommended();
}