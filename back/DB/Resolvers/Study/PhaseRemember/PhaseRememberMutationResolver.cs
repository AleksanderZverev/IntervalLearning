using Application.Common.Interfaces.Domain.Study.PhaseRemember;
using DB.Models;

namespace DB.Resolvers.Study.PhaseRemember;

public class PhaseRememberMutationResolver : BaseMutationResolver<PhaseRememberEntity>, IPhaseRememberMutationResolver
{
    public PhaseRememberMutationResolver(ApplicationContext db) : base(db)
    {
    }

    protected override void MarkAdded(PhaseRememberEntity entity)
    {
        db.PhaseRememberEntities.Add(entity);
    }

    protected override void MarkUpdated(PhaseRememberEntity entity)
    {
        db.PhaseRememberEntities.Update(entity);
    }

    protected override void MarkRemoved(PhaseRememberEntity entity)
    {
        db.PhaseRememberEntities.Remove(entity);
    }
}