using Application.Common.Interfaces.Domain.Collections;
using Application.Common.Interfaces.Domain.Store.PublicCollection;
using Application.Common.Interfaces.Domain.Store.PublicCollectionSubscribers;
using Application.Common.Interfaces.Domain.Themes;
using FluentResults;
using FluentResults.Extensions;
using Infrastructure.Errors;
using Infrastructure.Extensions;

namespace Application.Commands.Collections.SearchPublicCollection;

public class SearchPublicCollectionCommand : ICommand<SearchPublicCollectionRequest, List<SearchPublicCollectionItem>>
{
    private readonly IThemesQueryResolver themesQueryResolver;
    private readonly IPublicCollectionQueryResolver publicCollectionQueryResolver;
    private readonly IPublicCollectionSubscriberQueryResolver publicCollectionSubscriberQueryResolver;

    public SearchPublicCollectionCommand(
        IThemesQueryResolver themesQueryResolver,
        IPublicCollectionQueryResolver publicCollectionQueryResolver,
        IPublicCollectionSubscriberQueryResolver publicCollectionSubscriberQueryResolver)
    {
        this.themesQueryResolver = themesQueryResolver;
        this.publicCollectionQueryResolver = publicCollectionQueryResolver;
        this.publicCollectionSubscriberQueryResolver = publicCollectionSubscriberQueryResolver;
    }

    public async Task<Result<List<SearchPublicCollectionItem>>> Handle(SearchPublicCollectionRequest request)
    {
        var (myUserId, themeId, searchName, page, count) = request;

        return await themesQueryResolver.Find(themeId)
            .ToResultAsync()
            .ErrorIfNull(new BadRequestError("Specified theme is not found"))
            .Bind(async theme =>
            {
                var toSkip = (page - 1) * count;

                var foundCollections = await publicCollectionQueryResolver.Search(themeId, searchName, toSkip, count);

                var result = foundCollections
                    .Select(c =>
                    {
                        var subscription = publicCollectionSubscriberQueryResolver
                            .FindAsync(c.ParentUserId, c.Id, myUserId)
                            .GetAwaiter()
                            .GetResult();

                        return new SearchPublicCollectionItem(c, subscription);
                    })
                    .ToList();

                return result.ToResult();
            });
    }
}