using DB;
using DB.Models;
using Microsoft.EntityFrameworkCore;

namespace IntervalLearningApi.Services;

public class RepeatsScheduleService
{
    private readonly ApplicationContext db;

    public RepeatsScheduleService(ApplicationContext db)
    {
        this.db = db;
    }

    public List<RepeatsScheduleEntity> GetAll(long userId) 
        => db.RepeatsSchedules
            .Where(s => s.ParentUserId == userId)
            .Include(s => s.Phases)
            .AsNoTracking()
            .ToList();
}