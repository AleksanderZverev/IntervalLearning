using Application.Common.Interfaces.Domain.Cards;
using Domain.Card;
using FluentResults;
using FluentResults.Extensions;
using Infrastructure.Errors;
using Infrastructure.Extensions;

namespace Application.Commands.Cards.DeleteCard;

public class DeleteCardCommand : ICommand<DeleteCardRequest, Card>
{
    private readonly ICardsQueryResolver cardsQueryResolver;
    private readonly ICardsMutationResolver cardsMutationResolver;

    public DeleteCardCommand(
        ICardsQueryResolver cardsQueryResolver,
        ICardsMutationResolver cardsMutationResolver)
    {
        this.cardsQueryResolver = cardsQueryResolver;
        this.cardsMutationResolver = cardsMutationResolver;
    }

    public async Task<Result<Card>> Handle(DeleteCardRequest request)
    {
        return await cardsQueryResolver
            .Find(request.UserId, request.CollectionId, request.CardId)
            .ToResultAsync()
            .ErrorIfNull(new NotFoundError(nameof(Card)))
            .Bind(c => cardsMutationResolver.Delete(c));
    }
}