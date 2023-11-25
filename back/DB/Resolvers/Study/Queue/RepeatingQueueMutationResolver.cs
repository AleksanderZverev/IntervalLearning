using Application.Common.Interfaces.Domain.Study.Queue;
using DB.Configurations.Study;
using DB.Models.ValueObjects;
using Domain.Card;
using Domain.Queue;
using Domain.Schedule;
using FluentResults;

namespace DB.Resolvers.Study.Queue;

public class RepeatingQueueMutationResolver : BaseMutationResolver<CardRepeatQueue>, IRepeatingQueueMutationResolver
{
    public RepeatingQueueMutationResolver(ApplicationContext db) : base(db)
    {
    }

    public Result<QueueId> GetUniqueId(RepeatsSchedule schedule, Card card)
    {
        var queueSequenceName = CardRepeatQueueConfiguration.GetSequenceName(schedule, card);
        db.EnsureSequenceCreated(queueSequenceName);
        var nextValue = db.GetSequenceNextValue16(queueSequenceName);
        return QueueId.Create(nextValue);
    }

    protected override void MarkAdded(CardRepeatQueue entity)
    {
        db.Queue.Add(entity);
    }

    protected override void MarkUpdated(CardRepeatQueue entity)
    {
        db.Queue.Update(entity);
    }

    protected override void MarkRemoved(CardRepeatQueue entity)
    {
        db.Queue.Remove(entity);
    }
}