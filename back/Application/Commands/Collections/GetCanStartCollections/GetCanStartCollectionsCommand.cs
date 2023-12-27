using Application.Common.Interfaces.DB.Queries.Study;
using Domain.Common.ValueObjects;
using FluentResults;

namespace Application.Commands.Collections.GetCanStartCollections;

public class GetCanStartCollectionsCommand : ICommand<GetCanStartCollectionsCommandRequest, GetCanStartCollectionsResponse>
{
    private readonly IStudyQueryRepository studyQueryRepository;

    public GetCanStartCollectionsCommand(
        IStudyQueryRepository studyQueryRepository)
    {
        this.studyQueryRepository = studyQueryRepository;
    }

    public async Task<Result<GetCanStartCollectionsResponse>> Handle(GetCanStartCollectionsCommandRequest request)
    {
        var (userId, scheduleUserId, scheduleId, page, count) = request;

        var canStartCards = await studyQueryRepository.CardRemembers.GetCanStartCards(userId, scheduleUserId, scheduleId);

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

        var canStartCollections = await studyQueryRepository.Collections.GetRange(userId, collectionIdsToStart);

        var collectionToCardsCount = canStartCollections
            .GroupBy(c => c.Id)
            .ToDictionary(c => c.Key, c => canStartCards.Count(card => card.ParentCollectionId == c.Key));

        foreach (var collection in canStartCollections)
        {
            var notStartedCards = collectionToCardsCount[collection.Id];
            collection.NotStartedCardsCount = Counter.Create(notStartedCards).Value;
        }

        var canRelearnCardIds = await studyQueryRepository.RelearningCards.GetAll(userId);
        var canRelearnCollections = await studyQueryRepository.Collections.GetRange(
            userId,
            canRelearnCardIds.Select(c => c.CollectionId).Distinct().ToList());

        return new GetCanStartCollectionsResponse(totalCollectionsCount, canStartCollections, canRelearnCollections);
    }
}