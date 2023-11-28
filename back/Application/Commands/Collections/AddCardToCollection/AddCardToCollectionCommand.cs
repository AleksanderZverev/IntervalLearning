using Application.Commands.Cards.CreateCard;
using Application.Commands.Cards.UpdateCard;
using Application.Common.Interfaces.DB.Transactions;
using Application.Common.Interfaces.Domain.Collections;
using Domain.Card;
using Domain.Collection;
using FluentResults;
using Infrastructure.Errors;

namespace Application.Commands.Collections.AddCardToCollection;

public class AddCardToCollectionCommand : ICommand<AddCardToCollectionRequest, Card>
{
    private readonly ICollectionQueryResolver collectionQueryResolver;
    private readonly ICollectionMutationResolver collectionMutationResolver;
    private readonly CreateCardCommand createCardCommand;
    private readonly UpdateCardCommand updateCardCommand;
    private readonly ITransactionProvider transactionProvider;

    public AddCardToCollectionCommand(
        ICollectionQueryResolver collectionQueryResolver,
        ICollectionMutationResolver collectionMutationResolver,
        CreateCardCommand createCardCommand,
        UpdateCardCommand updateCardCommand,
        ITransactionProvider transactionProvider)
    {
        this.collectionQueryResolver = collectionQueryResolver;
        this.collectionMutationResolver = collectionMutationResolver;
        this.createCardCommand = createCardCommand;
        this.updateCardCommand = updateCardCommand;
        this.transactionProvider = transactionProvider;
    }

    public async Task<Result<Card>> Handle(AddCardToCollectionRequest request)
    {
        var (userId, collectionId, frontText, promptText, backText, description, examples) = request;
        
        var collection = await collectionQueryResolver.Find(userId, collectionId);

        if (collection == null)
            return new Error("Collection");
        
        using var transaction = transactionProvider.CreateScope();

        var createdCardResult = await createCardCommand.Handle(new CreateCardRequest()
        {
            ParentUserId = userId,
            ParentCollectionId = collectionId,
            MeaningText = backText,
            RememberingText = frontText,
            PromptText = promptText,
            Description = description,
            Examples = examples
        });

        if (createdCardResult.IsFailed)
        {
            return createdCardResult;
        }
        
        collection.CardsCount.Increment();
        var updateResult = collectionMutationResolver.Update(collection);
        
        if (updateResult.IsFailed)
            return new InternalError();

        transaction.Complete();
        return createdCardResult.Value;
    }
}