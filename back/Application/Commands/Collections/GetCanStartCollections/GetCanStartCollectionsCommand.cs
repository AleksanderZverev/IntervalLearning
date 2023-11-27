using Application.Common.Interfaces.Domain.Cards;
using Application.Common.Interfaces.Domain.Collections;
using Application.Common.Interfaces.Domain.Study.Remember;
using Domain.Common.ValueObjects;
using FluentResults;

namespace Application.Commands.Collections.GetCanStartCollections;

public class GetCanStartCollectionsCommand : ICommand<GetCanStartCollectionsRequest, GetCanStartCollectionsResponse>
{
    private readonly ICardsQueryResolver cardsQueryResolver;
    private readonly ICollectionQueryResolver collectionQueryResolver;
    private readonly IRememberQueryResolver rememberQueryResolver;

    public GetCanStartCollectionsCommand(
        ICardsQueryResolver cardsQueryResolver,
        ICollectionQueryResolver collectionQueryResolver,
        IRememberQueryResolver rememberQueryResolver)
    {
        this.cardsQueryResolver = cardsQueryResolver;
        this.collectionQueryResolver = collectionQueryResolver;
        this.rememberQueryResolver = rememberQueryResolver;
    }

    public async Task<Result<GetCanStartCollectionsResponse>> Handle(GetCanStartCollectionsRequest request)
    {
        var (userId, scheduleUserId, scheduleId, page, count) = request;

        var canStartCards = await rememberQueryResolver.GetCanStartCards(userId, scheduleUserId, scheduleId);

        var skip = (page - 1) * count;

        var canStartAllCollectionsIds = canStartCards
            .Select(c => c.ParentCollectionId)
            .Distinct()
            .ToList();

        var totalCollectionsCount = canStartAllCollectionsIds.Count;

        var collectionIdsToStart = canStartAllCollectionsIds
            .Skip(skip)
            .Take(count)
            .ToList();

        var canStartCollections = await collectionQueryResolver.GetRange(userId, collectionIdsToStart);

        var collectionToCardsCount = canStartCollections
            .GroupBy(c => c.Id)
            .ToDictionary(c => c.Key, c => canStartCards.Count(card => card.ParentCollectionId == c.Key));

        foreach (var collection in canStartCollections)
        {
            var notStartedCards = collectionToCardsCount[collection.Id];
            collection.NotStartedCardsCount = Counter.Create(notStartedCards).Value;
        }

        return new GetCanStartCollectionsResponse(totalCollectionsCount, canStartCollections);
    }
}