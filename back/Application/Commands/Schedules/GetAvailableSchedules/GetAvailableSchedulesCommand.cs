using Application.Common.Interfaces.DB.Repositories.Study;
using Domain.Schedule;
using FluentResults;

namespace Application.Commands.Schedules.GetAvailableSchedules;

public class GetAvailableSchedulesCommand : ICommand<GetAvailableSchedulesRequest, List<RepeatsSchedule>>
{
    private readonly IStudyQueryRepository studyQueryRepository;

    public GetAvailableSchedulesCommand(IStudyQueryRepository studyQueryRepository)
    {
        this.studyQueryRepository = studyQueryRepository;
    }

    public async Task<Result<List<RepeatsSchedule>>> Handle(GetAvailableSchedulesRequest request)
    {
        var userSchedules = await studyQueryRepository.Schedules.GetUsers(request.UserId);
        var recommendedSchedules = await studyQueryRepository.Schedules.GetRecommended();
        return userSchedules
            .Union(recommendedSchedules.Where(s => s.ParentUserId != request.UserId))
            .ToList();
    }
}