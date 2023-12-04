using Application.Commands.Cards.MoveCard;
using Application.Common.Interfaces.DB.Repositories.Study;
using Application.Common.Interfaces.DB.Transactions;
using Domain.Card;
using Domain.Collection;
using FluentResults;
using Infrastructure.Errors;

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

        sourceCollection.CardsCount.Decrement();
        // destinationCollection.CardsCount.Increment();

        studyRepository.Collections.Update(sourceCollection);
        studyRepository.Collections.Update(destinationCollection);
        
        var updatingResult = await studyRepository.SaveChangesAsync();
        if (updatingResult.IsFailed)
            return new InternalError();

        transaction.Complete();
        return movingResult.Value;
    }
}