using Application.Commands.Cards.MoveCard;
using Application.Common.Interfaces.DB.Transactions;
using Application.Common.Interfaces.Domain.Collections;
using Domain.Card;
using FluentResults;
using Infrastructure.Errors;

namespace Application.Commands.Collections.MoveCollectionCard;

public class MoveCollectionCardCommand : ICommand<MoveCollectionCardRequest, Card>
{
    private readonly ICollectionQueryResolver collectionQueryResolver;
    private readonly ICollectionMutationResolver collectionMutationResolver;
    private readonly ITransactionProvider transactionProvider;
    private readonly MoveCardCommand moveCardCommand;

    public MoveCollectionCardCommand(
        ICollectionQueryResolver collectionQueryResolver,
        ICollectionMutationResolver collectionMutationResolver,
        ITransactionProvider transactionProvider,
        MoveCardCommand moveCardCommand)
    {
        this.collectionQueryResolver = collectionQueryResolver;
        this.collectionMutationResolver = collectionMutationResolver;
        this.transactionProvider = transactionProvider;
        this.moveCardCommand = moveCardCommand;
    }

    public async Task<Result<Card>> Handle(MoveCollectionCardRequest request)
    {
        var (userId, sourceCollectionId, destinationCollectionId, cardId) = request;
        
        var sourceCollection = await collectionQueryResolver.Find(userId, sourceCollectionId);
        var destinationCollection = await collectionQueryResolver.Find(userId, destinationCollectionId);

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
        destinationCollection.CardsCount.Increment();
        
        var updatingResult = Result.Merge(
            collectionMutationResolver.Update(sourceCollection),
            collectionMutationResolver.Update(destinationCollection));

        if (updatingResult.IsFailed)
            return new InternalError();

        transaction.Complete();
        return movingResult.Value;
    }
}