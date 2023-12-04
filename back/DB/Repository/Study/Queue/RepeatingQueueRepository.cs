using Application.Common.Interfaces.DB.Repositories;
using Application.Common.Interfaces.DB.Repositories.Study.Queue;
using DB.Configurations.Study;
using DB.Repository;
using Domain.Queue;
using Domain.Queue.ValueObjects;
using FluentResults;

namespace DB.Resolvers.Study.Queue;

internal class RepeatingQueueRepository : BaseRepository<CardRepeatQueue>, IRepository<CardRepeatQueue, QueueId, RepeatingQueueIdParams>
{
    public RepeatingQueueRepository(ApplicationContext db) : base(db)
    {
    }

    public Result<QueueId> GetUniqueId(RepeatingQueueIdParams param)
    {
        var (schedule, card) = param;
        var queueSequenceName = CardRepeatQueueConfiguration.GetSequenceName(schedule, card);
        db.EnsureSequenceCreated(queueSequenceName);
        var nextValue = db.GetSequenceNextValue16(queueSequenceName);
        return QueueId.Create(nextValue);
    }
}