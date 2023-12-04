using Application.Common.Interfaces.DB.Repositories.Study;
using Application.Common.Interfaces.DB.Repositories.Study.Themes;
using Domain.Theme;
using FluentResults;
using Infrastructure.Errors;

namespace Application.Commands.Themes.CreateTheme;

public class CreateThemeCommand : ICommand<CreateThemeRequest>
{
    private readonly IStudyRepository studyRepository;

    public CreateThemeCommand(
        IStudyRepository studyRepository)
    {
        this.studyRepository = studyRepository;
    }

    public async Task<Result> Handle(CreateThemeRequest request)
    {
        var themeTitle = request.Title;
        
        var sameThemes = await studyRepository.Query.Themes.SearchByTitle(themeTitle);

        if (sameThemes is { Count: > 0 })
        {
            return new ConflictError("Theme");
        }
        
        var themeIdResult = studyRepository.Themes.GetUniqueId(new ThemeIdParams());

        if (themeIdResult.IsFailed)
        {
            return new InternalError();
        }
        
        var theme = new Theme(themeIdResult.Value)
        {
            Name = themeTitle
        };

        return studyRepository.Themes.UpdateAndSave(theme).ToResult();
    }
}