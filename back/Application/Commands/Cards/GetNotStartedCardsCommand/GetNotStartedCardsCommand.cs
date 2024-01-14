using Domain.Card;
using DomainServices.DB.Queries.Study;
using FluentResults;
using GlobalTools.Errors;

namespace Application.Commands.Cards.GetNotStartedCardsCommand;

public class GetNotStartedCardsCommand : ICommand<GetNotStartedCardsRequest, List<Card>>
{
    private readonly IStudyQueryRepository studyQueryRepository;

    public GetNotStartedCardsCommand(
        IStudyQueryRepository studyQueryRepository)
    {
        this.studyQueryRepository = studyQueryRepository;
    }

    public async Task<Result<List<Card>>> Handle(GetNotStartedCardsRequest request)
    {
        var schedule = await studyQueryRepository.Schedules.Find(request.ScheduleUserId, request.ScheduleId);

        if (schedule == null)
        {
            return new NotFoundError(nameof(schedule));
        }
        
        var startingRemembers = await studyQueryRepository.CardRemembers.GetRangeForCollection(
            request.UserId,
            request.CollectionId,
            request.ScheduleUserId,
            request.ScheduleId);
            
        var startedCardIds = startingRemembers
            .Select(c => c.ParentCardId)
            .ToList();

        var canStartCards = await studyQueryRepository.Cards.GetExceptRange(
            request.UserId,
            request.CollectionId,
            startedCardIds);

        return canStartCards
            .OrderBy(c => c.CreatedDate)
            .Take(request.Count)
            .ToList();
    }
}