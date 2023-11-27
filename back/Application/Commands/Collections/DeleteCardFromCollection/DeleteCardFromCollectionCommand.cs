using Application.Commands.Cards.DeleteCard;
using Application.Common.Interfaces.DB.Transactions;
using Application.Common.Interfaces.Domain.Collections;
using Domain.Card;
using Domain.Collection;
using FluentResults;
using FluentResults.Extensions;
using Infrastructure.Errors;
using Infrastructure.Extensions;

namespace Application.Commands.Collections.DeleteCardFromCollection;

public class DeleteCardFromCollectionCommand : ICommand<DeleteCardFromCollectionRequest, Card>
{
    private readonly ICollectionQueryResolver collectionQueryResolver;
    private readonly ICollectionMutationResolver collectionMutationResolver;
    private readonly DeleteCardCommand deleteCardCommand;
    private readonly ITransactionProvider transactionProvider;

    public DeleteCardFromCollectionCommand(
        ICollectionQueryResolver collectionQueryResolver,
        ICollectionMutationResolver collectionMutationResolver,
        DeleteCardCommand deleteCardCommand,
        ITransactionProvider transactionProvider)
    {
        this.collectionQueryResolver = collectionQueryResolver;
        this.collectionMutationResolver = collectionMutationResolver;
        this.deleteCardCommand = deleteCardCommand;
        this.transactionProvider = transactionProvider;
    }

    public async Task<Result<Card>> Handle(DeleteCardFromCollectionRequest request)
    {
        return await collectionQueryResolver
            .Find(request.UserId, request.CollectionId)
            .ToResultAsync()
            .ErrorIfNull(new NotFoundError(nameof(Collection)))
            .Bind(collection => DeletionDeleteCard(request, collection));
    }

    private async Task<Result<Card>> DeletionDeleteCard(
        DeleteCardFromCollectionRequest request,
        Collection collection)
    {
        using var transaction = transactionProvider.CreateScope();

        var deletionResult = await deleteCardCommand.Handle(
            new DeleteCardRequest(request.UserId, request.CollectionId, request.CardId));

        if (deletionResult.IsFailed)
        {
            return deletionResult;
        }

        collection.CardsCount.Decrement();
        
        var updateResult = collectionMutationResolver.Update(collection);

        if (updateResult.IsFailed)
        {
            return updateResult.ToResult();
        }

        transaction.Complete();
        return deletionResult.Value;
    }
}