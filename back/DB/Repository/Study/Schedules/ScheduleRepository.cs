using Application.Common.Interfaces.DB.Repositories;
using Application.Common.Interfaces.DB.Repositories.Study.Schedules;
using DB.Configurations.Study;
using DB.Models.ValueObjects;
using Domain.Schedule;
using FluentResults;

namespace DB.Repository.Study.Schedules;

public class ScheduleRepository : BaseRepository<RepeatsSchedule>, IRepository<RepeatsSchedule, ScheduleId, ScheduleIdParams>
{
    public ScheduleRepository(ApplicationContext db) : base(db)
    {
    }

    public Result<ScheduleId> GetUniqueId(ScheduleIdParams param)
    {
        var seqName = RepeatScheduleConfiguration.GetSequenceName(param.UserId);
        db.EnsureSequenceCreated(seqName);
        var nextId = db.GetSequenceNextValue16(seqName);
        return ScheduleId.Create(nextId);
    }
}