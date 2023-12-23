using Application.Common.Interfaces.DB.Queries.Study;
using Domain.Theme;
using FluentResults;
using Infrastructure.Extensions;

namespace Application.Commands.Themes.GetThemes;

public class GetThemesCommand : ICommand<GetThemesRequest, List<Theme>>
{
    private readonly IStudyQueryRepository studyQueryRepository;

    public GetThemesCommand(IStudyQueryRepository studyQueryRepository)
    {
        this.studyQueryRepository = studyQueryRepository;
    }


    public Task<Result<List<Theme>>> Handle(GetThemesRequest request)
    {
        return studyQueryRepository.Themes.GetAll().ToResultAsync();
    }
}