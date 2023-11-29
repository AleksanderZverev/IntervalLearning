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

    public Task<RepeatsSchedule?> Find(UserId userId, ScheduleId scheduleId)
    {
        return db.RepeatsSchedules
            .Include(s => s.Phases)
            .AsSplitQuery()
            .SingleOrDefaultAsync(s => s.ParentUserId == userId && s.Id ==scheduleId);
    }

    public Task<List<RepeatsSchedule>> GetUsers(UserId userId)
    {
        return db.RepeatsSchedules
            .Where(s => s.ParentUserId == userId)
            .Include(s => s.Phases)
            .AsSplitQuery()
            .ToListAsync();
    }
    
    public Task<List<RepeatsSchedule>> GetRecommended()
    {
        return db.RepeatsSchedules
            .Where(s => s.IsRecommended)
            .Include(s => s.Phases)
            .AsSplitQuery()
            .ToListAsync();
    }
}