using Application.Common.Interfaces.Domain.Cards;
using Application.Common.Interfaces.Domain.Study.Remember;
using Application.Common.Interfaces.Domain.Study.Schedule;
using Domain.Card;
using FluentResults;
using Infrastructure.Errors;

namespace Application.Commands.Cards.GetNotStartedCardsCommand;

public class GetNotStartedCardsCommand : ICommand<GetNotStartedCardsRequest, List<Card>>
{
    private readonly ICardsQueryResolver cardsQueryResolver;
    private readonly IScheduleResolver scheduleResolver;
    private readonly IRememberQueryResolver rememberQueryResolver;

    public GetNotStartedCardsCommand(
        ICardsQueryResolver cardsQueryResolver,
        IScheduleResolver scheduleResolver,
        IRememberQueryResolver rememberQueryResolver)
    {
        this.cardsQueryResolver = cardsQueryResolver;
        this.scheduleResolver = scheduleResolver;
        this.rememberQueryResolver = rememberQueryResolver;
    }

    public async Task<Result<List<Card>>> Handle(GetNotStartedCardsRequest request)
    {
        var schedule = await scheduleResolver.FindAsync(request.ScheduleUserId, request.ScheduleId);

        if (schedule == null)
        {
            return new NotFoundError(nameof(schedule));
        }
        
        var startingRemembers = await rememberQueryResolver.GetRangeForCollection(
            request.UserId,
            request.CollectionId,
            request.ScheduleUserId,
            request.ScheduleId);
            
        var startedCardIds = startingRemembers
            .Select(c => c.ParentCardId)
            .ToList();

        var canStartCards = await cardsQueryResolver.GetExceptRange(
            request.UserId,
            request.CollectionId,
            startedCardIds);

        return canStartCards.OrderBy(c => c.Id)
            .Take(request.Count)
            .ToList();
    }
}