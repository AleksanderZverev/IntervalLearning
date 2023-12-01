using Application.Commands.Cards.CreateCard;
using Application.Common.Interfaces.DB.Repositories.Study;
using Application.Common.Interfaces.DB.Transactions;
using Domain.Card;
using FluentResults;
using Infrastructure.Errors;

namespace Application.Commands.Collections.AddCardToCollection;

public class AddCardToCollectionCommand : ICommand<AddCardToCollectionRequest, Card>
{
    private readonly CreateCardCommand createCardCommand;
    private readonly IStudyRepository studyRepository;
    private readonly ITransactionProvider transactionProvider;

    public AddCardToCollectionCommand(
        CreateCardCommand createCardCommand,
        ITransactionProvider transactionProvider, 
        IStudyRepository studyRepository)
    {
        this.createCardCommand = createCardCommand;
        this.transactionProvider = transactionProvider;
        this.studyRepository = studyRepository;
    }

    public async Task<Result<Card>> Handle(AddCardToCollectionRequest request)
    {
        var (userId, collectionId, frontText, promptText, backText, description, examples) = request;
        
        var collection = await studyRepository.Query.Collections.Find(userId, collectionId);

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
        var updateResult = studyRepository.Collections.Update(collection);
        
        if (updateResult.IsFailed)
            return new InternalError();

        transaction.Complete();
        return createdCardResult.Value;
    }
}