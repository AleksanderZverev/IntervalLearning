using Application.Common.Interfaces.DB;
using DB.Models.ValueObjects;
using Domain.Card;
using Domain.Schedule;
using FluentResults;

namespace Application.Common.Interfaces.Domain.Study.Remember;

public interface IRememberMutationResolver : IMutationResolver<global::Domain.Schedule.Entities.Remember.Remember>
{
    public Result<RememberId> GetUniqueId(RepeatsSchedule schedule, Card card);
}