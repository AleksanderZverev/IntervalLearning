using Application.Common.Interfaces.DB.Queries.Store;
using Application.Common.Interfaces.DB.Repositories.Study;
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
    private readonly IStudyQueryRepository studyQueryRepository;
    private readonly IStoreQueryRepository storeQueryRepository;

    public SearchPublicCollectionCommand(
        IStudyQueryRepository studyQueryRepository,
        IStoreQueryRepository storeQueryRepository)
    {
        this.studyQueryRepository = studyQueryRepository;
        this.storeQueryRepository = storeQueryRepository;
    }

    public async Task<Result<List<SearchPublicCollectionItem>>> Handle(SearchPublicCollectionRequest request)
    {
        var (myUserId, themeId, searchName, page, count) = request;

        return await studyQueryRepository.Themes.Find(themeId)
            .ToResultAsync()
            .ErrorIfNull(new BadRequestError("Specified theme is not found"))
            .Bind(async theme =>
            {
                var toSkip = (page - 1) * count;

                var foundCollections = await storeQueryRepository.Collections.Search(themeId, searchName, toSkip, count);

                var result = foundCollections
                    .Select(c =>
                    {
                        var subscription = storeQueryRepository.Subscribers
                            .Find(c.ParentUserId, c.Id, myUserId)
                            .GetAwaiter()
                            .GetResult();

                        return new SearchPublicCollectionItem(c, subscription);
                    })
                    .ToList();

                return result.ToResult();
            });
    }
}