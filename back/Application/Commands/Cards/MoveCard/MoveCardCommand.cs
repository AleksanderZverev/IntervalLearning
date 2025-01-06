using Application.Commands.Cards.CreateCard;
using Application.Commands.Cards.DeleteCard;
using Domain.Card;
using Domain.Card.ValueObjects;
using Domain.Schedule.Entities.Remember;
using DomainServices.BoundedContext.Study.RememberService;
using DomainServices.DB.Repositories.Study;
using DomainServices.DB.Transactions;
using FluentResults;
using GlobalTools.Errors;

namespace Application.Commands.Cards.MoveCard;

public class MoveCardCommand : ICommand<MoveCardRequest, Card>
{
    private readonly IStudyRepository studyRepository;
    private readonly RememberService rememberService;
    private readonly ITransactionProvider transactionProvider;
    private readonly CreateCardCommand createCardCommand;
    private readonly DeleteCardCommand deleteCardCommand;

    public MoveCardCommand(
        RememberService rememberService,
        ITransactionProvider transactionProvider,
        CreateCardCommand createCardCommand,
        DeleteCardCommand deleteCardCommand,
        IStudyRepository studyRepository)
    {
        this.rememberService = rememberService;
        this.transactionProvider = transactionProvider;
        this.createCardCommand = createCardCommand;
        this.deleteCardCommand = deleteCardCommand;
        this.studyRepository = studyRepository;
    }

    public async Task<Result<Card>> Handle(MoveCardRequest request)
    {
        var (userId, sourceCollectionId, destinationCollectionId, cardId) = request;

        var card = await studyRepository.Query.Cards.Find(userId, sourceCollectionId, cardId);

        if (card == null)
            return new NotFoundError(nameof(Card));

        using var transaction = transactionProvider.CreateScope();

        var movedCardResult = await createCardCommand.Handle(
            new CreateCardRequest()
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
                Tags = card.Tags is { Count: > 0 }
                    ? card.Tags.ToList()
                    : new List<CardTag>(),
            });

        if (movedCardResult.IsFailed)
            return movedCardResult;

        var movedCard = movedCardResult.Value;
        movedCard.Remembers = card.Remembers
            .Select(r => rememberService.CreateForCard(r, movedCard))
            .ToList();

        studyRepository.CardRemembers.AddRange(movedCard.Remembers);

        var updateMovedCardResult = await studyRepository.SaveChangesAsync();
        if (updateMovedCardResult.IsFailed)
            return updateMovedCardResult;

        var deletionResult = await deleteCardCommand.Handle(new DeleteCardRequest(userId, sourceCollectionId, cardId));
        if (deletionResult.IsFailed)
            return deletionResult;

        transaction.Complete();
        return movedCard;
    }
}