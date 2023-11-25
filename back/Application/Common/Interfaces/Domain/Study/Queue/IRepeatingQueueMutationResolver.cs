using Application.Common.Interfaces.DB;
using DB.Models.ValueObjects;
using Domain.Card;
using Domain.Collection.ValueObjects;
using Domain.Queue;
using Domain.Schedule;
using Domain.User.ValueObjects;
using FluentResults;

namespace Application.Common.Interfaces.Domain.Study.Queue;

public interface IRepeatingQueueMutationResolver : IMutationResolver<CardRepeatQueue>
{
    public Result<QueueId> GetUniqueId(RepeatsSchedule schedule, Card card);
}