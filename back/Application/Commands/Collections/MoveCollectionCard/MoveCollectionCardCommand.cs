using Application.Commands.Cards.MoveCard;
using Domain.Card;
using DomainServices.DB.Repositories.Study;
using DomainServices.DB.Transactions;
using FluentResults;
using GlobalTools.Errors;

namespace Application.Commands.Collections.MoveCollectionCard;

public class MoveCollectionCardCommand : ICommand<MoveCollectionCardRequest, Card>
{
    private readonly IStudyRepository studyRepository;
    private readonly ITransactionProvider transactionProvider;
    private readonly MoveCardCommand moveCardCommand;

    public MoveCollectionCardCommand(
        ITransactionProvider transactionProvider,
        MoveCardCommand moveCardCommand,
        IStudyRepository studyRepository)
    {
        this.transactionProvider = transactionProvider;
        this.moveCardCommand = moveCardCommand;
        this.studyRepository = studyRepository;
    }

    public async Task<Result<Card>> Handle(MoveCollectionCardRequest request)
    {
        var (userId, sourceCollectionId, destinationCollectionId, cardId) = request;
        
        var sourceCollection = await studyRepository.Query.Collections.Find(userId, sourceCollectionId);
        var destinationCollection = await studyRepository.Query.Collections.Find(userId, destinationCollectionId);

        if (sourceCollection == null)
            return new NotFoundError("Source collection");
        if (destinationCollection == null)
            return new NotFoundError("Destination collection");

        using var transaction = transactionProvider.CreateScope();

        var movingResult = await moveCardCommand.Handle(
            new MoveCardRequest(userId, sourceCollectionId, destinationCollectionId, cardId));

        if (movingResult.IsFailed)
        {
            return movingResult;
        }

        transaction.Complete();
        return movingResult.Value;
    }
}