using Application.Common.Interfaces.Domain.Cards;
using Domain.Card;
using FluentResults;
using Infrastructure.Errors;

namespace Application.Commands.Cards;

public class GetCardCommand : ICommand<GetCardRequest, Card>
{
    private readonly ICardsQueryResolver cardsQueryResolver;

    public GetCardCommand(ICardsQueryResolver cardsQueryResolver)
    {
        this.cardsQueryResolver = cardsQueryResolver;
    }

    public async Task<Result<Card>> Handle(GetCardRequest request)
    {
        var card = await cardsQueryResolver.Find(
            request.UserId,
            request.CollectionId,
            request.CardId);

        return card == null
            ? new NotFoundError("Card")
            : card;
    }
}