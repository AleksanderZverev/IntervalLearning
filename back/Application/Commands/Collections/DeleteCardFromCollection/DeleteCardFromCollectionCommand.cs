using Application.Commands.Cards.DeleteCard;
using Application.Common.Interfaces.DB.Repositories.Study;
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
    private readonly IStudyRepository studyRepository;
    private readonly DeleteCardCommand deleteCardCommand;
    private readonly ITransactionProvider transactionProvider;

    public DeleteCardFromCollectionCommand(
        DeleteCardCommand deleteCardCommand,
        ITransactionProvider transactionProvider,
        IStudyRepository studyRepository)
    {
        this.deleteCardCommand = deleteCardCommand;
        this.transactionProvider = transactionProvider;
        this.studyRepository = studyRepository;
    }

    public async Task<Result<Card>> Handle(DeleteCardFromCollectionRequest request)
    {
        return await studyRepository.Query.Collections
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
        
        var updateResult = studyRepository.Collections.Update(collection);

        if (updateResult.IsFailed)
        {
            return updateResult.ToResult();
        }

        transaction.Complete();
        return deletionResult.Value;
    }
}