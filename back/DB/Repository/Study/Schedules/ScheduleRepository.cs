using DB.Configurations.Study;
using Domain.Schedule;
using Domain.Schedule.ValueObjects;
using Domain.User.ValueObjects;
using DomainServices.DB.Repositories;
using DomainServices.DB.Repositories.Study.Schedules;
using FluentResults;

namespace DB.Repository.Study.Schedules;

public class ScheduleRepository : BaseRepository<RepeatsSchedule>, IRepository<RepeatsSchedule, ScheduleId, ScheduleIdParams>
{
    public ScheduleRepository(ApplicationContext db) : base(db)
    {
    }

    private static string GetSequenceName(UserId userId)
        => $"schedule_of_user_{userId.Value}";

    public Result<ScheduleId> GetUniqueId(ScheduleIdParams param)
    {
        var seqName = GetSequenceName(param.UserId);
        const int schedulesStartValue = 100;
        db.EnsureSequenceCreated(seqName, schedulesStartValue);
        var nextId = db.GetSequenceNextValue16(seqName, schedulesStartValue);
        return ScheduleId.Create(nextId);
    }
}