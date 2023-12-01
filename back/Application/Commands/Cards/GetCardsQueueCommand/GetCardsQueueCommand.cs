using Application.Common.Interfaces.DB.Repositories.Study;
using Domain.Card;
using FluentResults;

namespace Application.Commands.Cards.GetCardsQueueCommand;

public class GetCardsQueueCommand : ICommand<GetCardsQueueRequest, List<Card>>
{
    private readonly IStudyQueryRepository studyQueryRepository;

    public GetCardsQueueCommand(
        IStudyQueryRepository studyQueryRepository)
    {
        this.studyQueryRepository = studyQueryRepository;
    }

    public async Task<Result<List<Card>>> Handle(GetCardsQueueRequest request)
    {
        var queueItems = await studyQueryRepository.RepeatingQueue.GetByDate(
            request.UserId,
            request.CollectionId,
            request.ScheduleUserId,
            request.ScheduleId,
            request.PhaseIndex,
            request.DateTime);
        
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