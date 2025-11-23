using Domain.Card;
using Domain.Queue;
using DomainServices.DB.Queries.Study;
using FluentResults;
using GlobalTools;
using GlobalTools.Errors;

namespace Application.Commands.Cards.GetCardsQueueCommand;

public record GetCardsQueueCommandResponse(
    List<Card> Cards,
    int TotalCardsCount);

public class GetCardsQueueCommand : ICommand<GetCardsQueueRequest, GetCardsQueueCommandResponse>
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

    public async Task<Result<GetCardsQueueCommandResponse>> Handle(GetCardsQueueRequest request)
    {
        var schedule = await studyQueryRepository.Schedules.Find(request.ScheduleUserId, request.ScheduleId);

        if (schedule == null)
            return new BadRequestError("Schedule is not found");

        var (queueItems, totalCardsCount) = await studyQueryRepository.RepeatingQueue.GetByDate(
            request.Page,
            request.CardsCountByPage,
            request.UserId,
            request.CollectionId,
            request.ScheduleUserId,
            request.ScheduleId,
            request.Date);

        IEnumerable<CardRepeatQueue> filteredQueueItems = queueItems;

        if (request.IsRepeatingMode)
            filteredQueueItems = filteredQueueItems
                .Where(q => schedule.GetPhaseOrThrow(q.PhaseIndex).IsRepeat());

        if (request.CheckRepeatableDate)
            filteredQueueItems = filteredQueueItems
                .Where(q => schedule.CanRepeat(q.PhaseIndex, request.Date, dateTimeProvider).Value);

        var repeatingQueueItems = filteredQueueItems.ToList();

        if (repeatingQueueItems.Count == 0)
            return new GetCardsQueueCommandResponse([], 0);

        var cardsIds = repeatingQueueItems.Select(q => q.ParentCardId).ToList();

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

        return new GetCardsQueueCommandResponse(cards, totalCardsCount);
    }
}