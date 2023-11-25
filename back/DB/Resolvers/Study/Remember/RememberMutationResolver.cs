using Application.Common.Interfaces.Domain.Study.Remember;
using DB.Configurations.Study;
using DB.Models.ValueObjects;
using Domain.Card;
using Domain.Card.ValueObjects;
using Domain.Schedule;
using FluentResults;

namespace DB.Resolvers.Study.Remember;

public class RememberMutationResolver : BaseMutationResolver<Domain.Schedule.Entities.Remember.Remember>, IRememberMutationResolver
{
    public RememberMutationResolver(ApplicationContext db) : base(db)
    {
    }

    protected override void MarkAdded(Domain.Schedule.Entities.Remember.Remember entity)
    {
        db.Remembers.Add(entity);
    }

    protected override void MarkUpdated(Domain.Schedule.Entities.Remember.Remember entity)
    {
        db.Remembers.Update(entity);
    }

    protected override void MarkRemoved(Domain.Schedule.Entities.Remember.Remember entity)
    {
        db.Remembers.Remove(entity);
    }

    public Result<RememberId> GetUniqueId(RepeatsSchedule schedule, Card card)
    {
        var sequenceName = RememberConfiguration.GetSequenceName(
            new ComplexScheduleId()
            {
                ParentUserId = schedule.ParentUserId,
                Id = schedule.Id,
            },
            new ComplexCardId()
            {
                UserId = card.ParentUserId,
                CollectionId = card.ParentCollectionId,
                Id = card.Id,
            });
        
        db.EnsureSequenceCreated(sequenceName);
        var nextValue = db.GetSequenceNextValue16(sequenceName);
        return RememberId.Create(nextValue);
    }
}