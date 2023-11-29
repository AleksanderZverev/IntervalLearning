using Application.Common.Interfaces.DB.Repositories.Study;
using Application.Common.Interfaces.Domain.Collections;
using Application.Common.Interfaces.Domain.Themes;
using Domain.Collection;
using FluentResults;
using FluentResults.Extensions;
using Infrastructure.Errors;
using Infrastructure.Extensions;

namespace Application.Commands.Collections.SearchCollection;

public class SearchCollectionCommand : ICommand<SearchCollectionRequest, List<Collection>>
{
    private readonly IStudyQueryRepository studyQueryRepository;

    public SearchCollectionCommand(
        IStudyQueryRepository studyQueryRepository)
    {
        this.studyQueryRepository = studyQueryRepository;
    }

    public async Task<Result<List<Collection>>> Handle(SearchCollectionRequest request)
    {
        var (userId, themeId, searchName, page, count) = request;
        var toSkip = (page - 1) * count;

        return await studyQueryRepository.Themes
            .Find(themeId)
            .ToResultAsync()
            .ErrorIfNull(new BadRequestError("Incorrect theme id"))
            .Bind(theme => studyQueryRepository.Collections.Search(userId, themeId, searchName, toSkip, count));
    }
}