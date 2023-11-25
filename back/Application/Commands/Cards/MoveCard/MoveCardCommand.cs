using Application.Commands.Cards.CreateCard;
using Application.Commands.Cards.DeleteCard;
using Application.Common.Interfaces.DB.Transactions;
using Application.Common.Interfaces.Domain.Cards;
using Domain.Card;
using Domain.Card.ValueObjects;
using Domain.Schedule.Entities.Remember;
using FluentResults;
using Infrastructure.Errors;

namespace Application.Commands.Cards.MoveCard;

public class MoveCardCommand : ICommand<MoveCardRequest, Card>
{
    private readonly ICardsQueryResolver cardsQueryResolver;
    private readonly ICardsMutationResolver cardsMutationResolver;
    private readonly ITransactionProvider transactionProvider;
    private readonly CreateCardCommand createCardCommand;
    private readonly DeleteCardCommand deleteCardCommand;

    public MoveCardCommand(
        ICardsQueryResolver cardsQueryResolver,
        ICardsMutationResolver cardsMutationResolver,
        ITransactionProvider transactionProvider,
        CreateCardCommand createCardCommand,
        DeleteCardCommand deleteCardCommand)
    {
        this.cardsQueryResolver = cardsQueryResolver;
        this.cardsMutationResolver = cardsMutationResolver;
        this.transactionProvider = transactionProvider;
        this.createCardCommand = createCardCommand;
        this.deleteCardCommand = deleteCardCommand;
    }

    public async Task<Result<Card>> Handle(MoveCardRequest request)
    {
        var (userId, sourceCollectionId, destinationCollectionId, cardId) = request;
        
        var card = await cardsQueryResolver.Find(userId, sourceCollectionId, cardId);

        if (card == null)
            return new NotFoundError(nameof(Card));
        
        using var transaction = transactionProvider.CreateScope();

        var movedCardResult = await createCardCommand.Handle(new CreateCardRequest()
        {
            ParentUserId = userId,
            ParentCollectionId = destinationCollectionId,
            RememberingText = card.RememberingText,
            PromptText = card.PromptText,
            MeaningText = card.MeaningText,
            Description = card.Description,
            Examples = card.Examples is { Count: > 0 }
                ? card.Examples.ToList()
                : new List<CardExample>(),

        });

        if (movedCardResult.IsFailed)
            return movedCardResult;

        var movedCard = movedCardResult.Value;
        movedCard.CreatedDate = card.CreatedDate;
        
        movedCard.Remembers = card.Remembers
            .Select(r => new Remember(
                r.ParentRepeatsScheduleUserId,
                r.ParentRepeatsScheduleId,
                movedCard.ParentUserId,
                movedCard.ParentCollectionId,
                movedCard.Id,
                r.Id,
                r.Weight,
                r.PhaseIndex,
                r.RepeatedDate))
            .ToList();
        
        var updateMovedCardResult = cardsMutationResolver.Update(movedCard);

        if (updateMovedCardResult.IsFailed)
            return updateMovedCardResult;

        var deletionResult = await deleteCardCommand.Handle(
            new DeleteCardRequest(userId, sourceCollectionId, cardId));

        if (deletionResult.IsFailed)
        {
            return deletionResult;
        }

        transaction.Complete();
        return movedCard;
    }
}