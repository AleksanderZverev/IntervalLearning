using Domain.Collection.ValueObjects;
using Domain.User.ValueObjects;
using DomainServices.DB.Queries.Study;
using FluentResults;
using GlobalTools.Extensions;

namespace Application.Commands.Collections.GetCollectionStatistic;

public record GetCollectionStatisticCommandRequest(
    UserId UserId,
    CollectionId CollectionId,
    DateTimeOffset UserCurrentDateTime);

public record GetCollectionStatisticCommandResponse(int TodayAddedCards, int StartedLearningCards);

public class GetCollectionStatisticCommand : ICommand<
    GetCollectionStatisticCommandRequest,
    GetCollectionStatisticCommandResponse>
{
    private readonly IStudyQueryRepository studyQueryRepository;

    public GetCollectionStatisticCommand(IStudyQueryRepository studyQueryRepository)
    {
        this.studyQueryRepository = studyQueryRepository;
    }

    public async Task<Result<GetCollectionStatisticCommandResponse>> Handle(
        GetCollectionStatisticCommandRequest request)
    {
        var (userId, collectionId, userCurrentDateTime) = request;

        var (dateFrom, dateTo) = userCurrentDateTime.GetDateRange();
        var todayAddedCardCount =
            await studyQueryRepository.Cards.CountByDateRange(userId, collectionId, dateFrom, dateTo);
        var startedLearningCardsCount = await studyQueryRepository.Cards.CountStartedLearning(userId, collectionId);
        return new GetCollectionStatisticCommandResponse(
            TodayAddedCards: todayAddedCardCount,
            StartedLearningCards: startedLearningCardsCount);
    }
}