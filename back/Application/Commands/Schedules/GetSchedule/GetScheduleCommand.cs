using Application.Common.Interfaces.DB.Repositories.Study;
using Domain.Schedule;
using FluentResults;
using Infrastructure.Errors;
using Infrastructure.Extensions;

namespace Application.Commands.Schedules.GetSchedule;

public class GetScheduleCommand : ICommand<GetScheduleRequest, RepeatsSchedule>
{
    private readonly IStudyQueryRepository studyQueryRepository;

    public GetScheduleCommand(IStudyQueryRepository studyQueryRepository)
    {
        this.studyQueryRepository = studyQueryRepository;
    }

    public Task<Result<RepeatsSchedule>> Handle(GetScheduleRequest request)
    {
        return studyQueryRepository.Schedules
            .Find(request.UserId, request.ScheduleId)
            .ToResultAsync()
            .ErrorIfNull(new NotFoundError("Schedule"));
    }
}