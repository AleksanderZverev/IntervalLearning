using Application.Common.Interfaces.Domain.Cards;
using Domain.Card;
using FluentResults;

namespace Application.Commands.Cards.CreateCard;

public class CreateCardCommand : ICommand<CreateCardRequest, Card>
{
    private readonly ICardsMutationResolver cardsMutationResolver;

    public CreateCardCommand(ICardsMutationResolver cardsMutationResolver)
    {
        this.cardsMutationResolver = cardsMutationResolver;
    }

    public async Task<Result<Card>> Handle(CreateCardRequest request)
    {
        return Result.Ok()
            .Bind(() => cardsMutationResolver.GetUniqueId(request.ParentUserId, request.ParentCollectionId))
            .Bind(cardId =>
            {
                var card = new Card(request.ParentUserId, request.ParentCollectionId, cardId)
                {
                    MeaningText = request.MeaningText,
                    RememberingText = request.RememberingText,
                    PromptText = request.PromptText,
                    Description = request.Description,
                };

                if (request.Examples is { Count: > 0 })
                {
                    card.Examples = request.Examples;
                }

                return Result.Ok(card);
            })
            .Bind(card => cardsMutationResolver.Add(card));
    }
}