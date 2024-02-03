using Domain.Schedule.Entities.Phase.Entities;
using Domain.Schedule.Entities.Phase.ValueObjects;
using Domain.User.ValueObjects;
using DomainServices.DB.Repositories;
using DomainServices.DB.Repositories.Study.PhaseRemembers;
using FluentResults;

namespace DB.Repository.Study.PhaseRemember;

internal class PhaseRememberRepository : BaseRepository<PhaseRememberEntity>,
    IRepository<PhaseRememberEntity, int, PhaseRememberIdParams>
{
    public PhaseRememberRepository(ApplicationContext db) : base(db)
    {
    }

    private static string GetSequenceName(ComplexPhaseId phaseId, UserId repeatedUserId)
    {
        return $"phase_remembers_" +
               $"phase_{phaseId.ParentUserId}_{phaseId.ParentRepeatsScheduleId}_{phaseId.Id}_" +
               $"user_{repeatedUserId}";
    }

    public Result<int> GetUniqueId(PhaseRememberIdParams param)
    {
        var (phase, userId) = param;
        var sequenceName = GetSequenceName(
            new ComplexPhaseId()
            {
                ParentUserId = phase.ParentUserId,
                ParentRepeatsScheduleId = phase.ParentRepeatsScheduleId,
                Id = phase.Id,
            },
            userId);

        const int minimalId = short.MaxValue + 1; 
        db.EnsureSequenceCreated(sequenceName, minimalId);
        var nextValue = db.GetSequenceNextValue32(sequenceName, minimalId);
        return nextValue;
    }
}