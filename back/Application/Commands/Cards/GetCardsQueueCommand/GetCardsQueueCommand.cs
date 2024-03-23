using Domain.Card;
using DomainServices.DB.Queries.Study;
using FluentResults;
using GlobalTools;
using GlobalTools.Errors;

namespace Application.Commands.Cards.GetCardsQueueCommand;

public class GetCardsQueueCommand : ICommand<GetCardsQueueRequest, List<Card>>
{
    private readonly IDateTimeProvider dateTimeProvider;
    private readonly IStudyQueryRepository studyQueryRepository;

    public GetCardsQueueCommand(
        IDateTimeProvider dateTimeProvider,
        IStudyQueryRepository studyQueryRepository)
    {
        this.dateTimeProvider = dateTimeProvider;
        this.studyQueryRepository = studyQueryRepository;
    }

    public async Task<Result<List<Card>>> Handle(GetCardsQueueRequest request)
    {
        var schedule = await studyQueryRepository.Schedules.Find(request.ScheduleUserId, request.ScheduleId);

        if (schedule == null)
            return new BadRequestError("Schedule is not found");

        var canRepeatResult = schedule.CanRepeat(request.PhaseIndex, request.Date, dateTimeProvider);

        if (canRepeatResult.IsFailed)
            return canRepeatResult.ToResult();

        var canRepeat = canRepeatResult.Value;

        if (request.CheckRepeatableDate && !canRepeat)
        {
            return new BadRequestError("Not repeatable date requested");
        }

        var queueItems = await studyQueryRepository.RepeatingQueue.GetByDate(
            request.UserId,
            request.CollectionId,
            request.ScheduleUserId,
            request.ScheduleId,
            request.PhaseIndex,
            request.Date);

        if (queueItems.Count == 0)
            return new List<Card>(0);

        var cardsIds = queueItems.Select(q => q.ParentCardId).ToList();

        var cards = await studyQueryRepository.Cards.GetRange(request.UserId, request.CollectionId, cardsIds);

        var remembers = await studyQueryRepository.CardRemembers.GetRangeForCards(
            request.UserId,
            request.CollectionId,
            request.ScheduleUserId,
            request.ScheduleId,
            cardsIds);

        var cardIdToRemember = remembers
            .GroupBy(r => r.ParentCardId)
            .ToDictionary(g => g.Key, g => g.ToList());

        foreach (var card in cards)
        {
            var cardsRemembers = cardIdToRemember[card.Id];
            card.Remembers = cardsRemembers;
        }

        return cards;
    }
}