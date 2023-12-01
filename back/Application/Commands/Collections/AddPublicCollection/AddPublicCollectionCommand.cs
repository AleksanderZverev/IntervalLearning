using System.Diagnostics;
using Application.Commands.Collections.AddCardToCollection;
using Application.Commands.Collections.CreateCollection;
using Application.Common.Interfaces.DB.Repositories.Store;
using Application.Common.Interfaces.DB.Repositories.Study;
using Application.Common.Interfaces.DB.Transactions;
using DB.Models.Store;
using Domain.Card;
using Domain.Collection;
using Domain.Collection.ValueObjects;
using FluentResults;
using Infrastructure.Errors;

namespace Application.Commands.Collections.AddPublicCollection;

public class AddPublicCollectionCommand : ICommand<AddPublicCollectionRequest, Collection>
{
    private readonly ITransactionProvider transactionProvider;
    private readonly IStoreRepository storeRepository;
    private readonly CreateCollectionCommand createCollectionCommand;
    private readonly AddCardToCollectionCommand addCardToCollectionCommand;
    private readonly IStudyQueryRepository studyQueryRepository;

    public AddPublicCollectionCommand(
        ITransactionProvider transactionProvider,
        CreateCollectionCommand createCollectionCommand,
        AddCardToCollectionCommand addCardToCollectionCommand,
        IStudyQueryRepository studyQueryRepository,
        IStoreRepository storeRepository)
    {
        this.transactionProvider = transactionProvider;
        this.createCollectionCommand = createCollectionCommand;
        this.addCardToCollectionCommand = addCardToCollectionCommand;
        this.studyQueryRepository = studyQueryRepository;
        this.storeRepository = storeRepository;
    }

    public async Task<Result<Collection>> Handle(AddPublicCollectionRequest request)
    {
        var (publicCollectionUserId, publicCollectionId, myUserId, myCollectionId, newCollectionName, checkUnique) = request;

        if (myCollectionId == null && string.IsNullOrEmpty(newCollectionName))
            return new BadRequestError();

        var publicCollection = await storeRepository.Query.Collections.Find(publicCollectionUserId, publicCollectionId);

        if (publicCollection == null)
        {
            return new NotFoundError("Public collection");
        }

        using var transaction = transactionProvider.CreateScope();

        Collection myCollection;

        if (myCollectionId != null)
        {
            var foundCollection = await studyQueryRepository.Collections.Find(myUserId, myCollectionId);

            if (foundCollection == null)
                return new NotFoundError("Specified personal collection");

            myCollection = foundCollection;
        }
        else
        {
            var createdCollectionResult = await createCollectionCommand.Handle(new CreateCollectionRequest()
            {
                Title = CollectionTitle.Create(newCollectionName).Value,
                ParentUserId = myUserId,
                ThemeId = publicCollection.ThemeId,
            });

            if (createdCollectionResult.IsFailed)
                return createdCollectionResult;

            myCollection = createdCollectionResult.Value;
        }
        
        if (publicCollection.ThemeId != myCollection.ThemeId)
        {
            return new BadRequestError("Themes of collections are different");
        }

        var publicCards = await studyQueryRepository.Cards.GetAll(publicCollectionUserId, publicCollectionId);

        if (publicCards.Count == 0)
        {
            return new BadRequestError("No cards in the public collection");
        }

        var myCards = checkUnique 
            ? await studyQueryRepository.Cards.GetAll(myUserId, myCollection.Id) 
            : new List<Card>();
        
        var myCardsSet = new HashSet<string>(myCards.Select(c => c.RememberingText.Value));

        foreach (var publicCard in publicCards)
        {
            if (checkUnique && myCardsSet.Contains(publicCard.RememberingText))
            {
                continue;
            }
            
            var addedCardResult = await addCardToCollectionCommand.Handle(new AddCardToCollectionRequest(
                myUserId,
                myCollection.Id,
                publicCard.RememberingText,
                publicCard.PromptText,
                publicCard.MeaningText,
                publicCard.Description,
                publicCard.Examples));

            if (addedCardResult.IsFailed)
            {
                return addedCardResult.ToResult();
            }
        }

        var publication = await storeRepository.Query.Publications.Find(publicCollectionUserId, publicCollectionId);

        if (publication == null)
        {
            Debug.Fail("publication == null");
            return new InternalError();
        }

        var subscriber = await storeRepository.Query.Subscribers.Find(publicCollectionUserId, publicCollectionId, myUserId);

        if (subscriber == null)
        {
            var addSubscriberResult = storeRepository.Subscribers.Add(new PublicCollectionSubscriber()
            {
                ParentUserId = publicCollectionUserId,
                ParentCollectionId = publicCollectionId,
                SubscriberUserId = myUserId,
                IsAdded = true,
            });

            if (addSubscriberResult.IsFailed)
            {
                return new InternalError();
            }
        }
        else
        {
            subscriber.IsAdded = true;
            var updateSubscriberResult = storeRepository.Subscribers.Update(subscriber);

            if (updateSubscriberResult.IsFailed)
            {
                return new InternalError();
            }
        }
        
        publication.SubscribersCount++;
        var updatePublicationResult = storeRepository.Publications.Update(publication);

        if (updatePublicationResult.IsFailed)
        {
            return new InternalError();
        }

        transaction.Complete();
        return myCollection;
    }
}