using Domain.Theme;
using DomainServices.DB.Repositories.Study;
using FluentResults;
using FluentResults.Extensions;
using GlobalTools.Errors;
using GlobalTools.Extensions;

namespace Application.Commands.Themes.UpdateTheme;

public class UpdateThemeCommand : ICommand<UpdateThemeRequest, Theme>
{
    private readonly IStudyRepository studyRepository;

    public UpdateThemeCommand(IStudyRepository studyRepository)
    {
        this.studyRepository = studyRepository;
    }

    public async Task<Result<Theme>> Handle(UpdateThemeRequest request)
    {
        return await studyRepository.Query.Themes.Find(request.Id)
            .ToResultAsync()
            .ErrorIfNull(new NotFoundError(nameof(Theme)))
            .Bind(theme =>
            {
                theme.Update(request.Title);
                return studyRepository.Themes.UpdateAndSave(theme);
            });
    }
}
