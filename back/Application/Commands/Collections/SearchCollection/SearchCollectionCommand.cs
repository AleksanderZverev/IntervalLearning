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
    private readonly IThemesQueryResolver themesQuery;
    private readonly ICollectionQueryResolver collectionQuery;

    public SearchCollectionCommand(
        IThemesQueryResolver themesQuery,
        ICollectionQueryResolver collectionQuery)
    {
        this.themesQuery = themesQuery;
        this.collectionQuery = collectionQuery;
    }

    public async Task<Result<List<Collection>>> Handle(SearchCollectionRequest request)
    {
        var (userId, themeId, searchName, page, count) = request;
        var toSkip = (page - 1) * count;

        return await themesQuery
            .Find(themeId)
            .ToResultAsync()
            .ErrorIfNull(new BadRequestError("Incorrect theme id"))
            .Bind(theme => collectionQuery.Search(userId, themeId, searchName, toSkip, count));
    }
}