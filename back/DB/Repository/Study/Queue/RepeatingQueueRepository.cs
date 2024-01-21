using DB.Configurations.Study;
using DB.Repository;
using Domain.Card;
using Domain.Queue;
using Domain.Queue.ValueObjects;
using Domain.Schedule;
using DomainServices.DB.Repositories;
using DomainServices.DB.Repositories.Study.Queue;
using FluentResults;

namespace DB.Resolvers.Study.Queue;

internal class RepeatingQueueRepository : BaseRepository<CardRepeatQueue>, IRepository<CardRepeatQueue, QueueId, RepeatingQueueIdParams>
{
    public RepeatingQueueRepository(ApplicationContext db) : base(db)
    {
    }

    private static string GetSequenceName(RepeatsSchedule scheduleWithPhases, Card card)
    {
        return $"queue_" +
               $"schedule_{scheduleWithPhases.ParentUserId.Value}_{scheduleWithPhases.Id}_" +
               $"card_{card.ParentUserId.Value}_{card.ParentCollectionId.Value}_{card.Id.Value}";
    }

    public Result<QueueId> GetUniqueId(RepeatingQueueIdParams param)
    {
        var (schedule, card) = param;
        var queueSequenceName = GetSequenceName(schedule, card);
        db.EnsureSequenceCreated(queueSequenceName);
        var nextValue = db.GetSequenceNextValue16(queueSequenceName);
        return QueueId.Create(nextValue);
    }
}