using Domain.Theme;
using DomainServices.DB.Repositories.Study;
using DomainServices.DB.Repositories.Study.Themes;
using FluentResults;
using GlobalTools.Errors;

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