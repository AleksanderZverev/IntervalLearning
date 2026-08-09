using Domain.Theme;
using DomainServices.DB.Repositories.Study;
using FluentResults;
using FluentResults.Extensions;
using GlobalTools.Errors;
using GlobalTools.Extensions;

namespace Application.Commands.Themes.DeleteTheme;

public class DeleteThemeCommand : ICommand<DeleteThemeRequest>
{
    private readonly IStudyRepository studyRepository;

    public DeleteThemeCommand(IStudyRepository studyRepository)
    {
        this.studyRepository = studyRepository;
    }

    public async Task<Result> Handle(DeleteThemeRequest request)
    {
        return await studyRepository.Query.Themes.Find(request.Id)
            .ToResultAsync()
            .ErrorIfNull(new NotFoundError(nameof(Theme)))
            .Bind(theme => studyRepository.Themes.DeleteAndSave(theme).ToResult());
    }
}
