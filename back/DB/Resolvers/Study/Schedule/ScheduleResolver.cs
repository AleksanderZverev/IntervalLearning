using Application.Common.Interfaces.Domain.Study.Schedule;
using DB.Models.ValueObjects;
using Domain.Schedule;
using Domain.User.ValueObjects;

namespace DB.Resolvers.Study.Schedule;

public class ScheduleResolver : IScheduleResolver
{
    private readonly ApplicationContext db;

    public ScheduleResolver(ApplicationContext db)
    {
        this.db = db;
    }

    public Task<RepeatsSchedule?> FindAsync(UserId userId, ScheduleId scheduleId)
    {
        return db.RepeatsSchedules.FindAsync(userId, scheduleId).AsTask();
    }
}