using Domain.Schedule;
using Domain.Schedule.ValueObjects;
using Domain.User.ValueObjects;

namespace DomainServices.DB.Queries.Study.Schedule;

public interface IScheduleResolver
{
    Task<RepeatsSchedule?> Find(UserId userId, ScheduleId scheduleId);
    Task<List<RepeatsSchedule>> GetUsers(UserId userId);
    Task<List<RepeatsSchedule>> GetRecommended();
}