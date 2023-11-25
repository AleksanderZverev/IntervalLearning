using Application.Commands.Cards.GetAllCards;
using Application.Common.Interfaces.Domain.Cards;
using Application.Common.Interfaces.Domain.Study.Queue;
using Application.Common.Interfaces.Domain.Study.Remember;
using Domain.Card;
using FluentResults;

namespace Application.Commands.Cards.GetCardsQueueCommand;

public class GetCardsQueueCommand : ICommand<GetCardsQueueRequest, List<Card>>
{
    private readonly ICardsQueryResolver cardsQueryResolver;
    private readonly IRepeatingQueueResolver queueResolver;
    private readonly IRememberQueryResolver rememberQueryResolver;

    public GetCardsQueueCommand(
        ICardsQueryResolver cardsQueryResolver,
        IRepeatingQueueResolver queueResolver,
        IRememberQueryResolver rememberQueryResolver)
    {
        this.cardsQueryResolver = cardsQueryResolver;
        this.queueResolver = queueResolver;
        this.rememberQueryResolver = rememberQueryResolver;
    }

    public async Task<Result<List<Card>>> Handle(GetCardsQueueRequest request)
    {
        var queueItems = await queueResolver.GetByDate(
            request.UserId,
            request.CollectionId,
            request.ScheduleUserId,
            request.ScheduleId,
            request.PhaseIndex,
            request.DateTime);
        
        if (queueItems.Count == 0)
            return new List<Card>(0);

        var cardsIds = queueItems.Select(q => q.ParentCardId).ToList();

        var cards = await cardsQueryResolver.GetRange(request.UserId, request.CollectionId, cardsIds);

        var remembers = await rememberQueryResolver.GetRangeForCards(
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