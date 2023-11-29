using Application.Common.Interfaces.Domain.Study.Schedule;
using DB.Models.ValueObjects;
using Domain.Schedule;
using Domain.User.ValueObjects;
using Microsoft.EntityFrameworkCore;

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
        return db.RepeatsSchedules
            .Include(s => s.Phases)
            .SingleOrDefaultAsync(s => s.ParentUserId == userId && s.Id ==scheduleId);
    }
}