using Domain.Collection;
using DomainServices.DB.Queries.Study;
using FluentResults;
using FluentResults.Extensions;
using GlobalTools.Errors;
using GlobalTools.Extensions;

namespace Application.Commands.Collections.SearchCollection;

public class SearchCollectionCommand : ICommand<SearchCollectionCommandRequest, List<Collection>>
{
    private readonly IStudyQueryRepository studyQueryRepository;

    public SearchCollectionCommand(
        IStudyQueryRepository studyQueryRepository)
    {
        this.studyQueryRepository = studyQueryRepository;
    }

    public async Task<Result<List<Collection>>> Handle(SearchCollectionCommandRequest request)
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