using System.Diagnostics;
using Application.Commands.Collections.AddCardToCollection;
using Application.Commands.Collections.CreateCollection;
using Application.Common.Interfaces.DB.Transactions;
using Application.Common.Interfaces.Domain.Cards;
using Application.Common.Interfaces.Domain.Collections;
using Application.Common.Interfaces.Domain.Store.CollectionPublications;
using Application.Common.Interfaces.Domain.Store.PublicCollection;
using Application.Common.Interfaces.Domain.Store.PublicCollectionSubscribers;
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
    private readonly ICollectionQueryResolver collectionQueryResolver;
    private readonly ICardsQueryResolver cardsQueryResolver;
    private readonly IPublicCollectionQueryResolver publicCollectionQueryResolver;
    private readonly ICollectionPublicationQueryResolver collectionPublicationQueryResolver;
    private readonly IPublicCollectionSubscriberQueryResolver subscriberQueryResolver;
    private readonly ICollectionPublicationMutationResolver collectionPublicationMutationResolver;
    private readonly IPublicCollectionSubscriberMutationResolver publicCollectionSubscriberMutationResolver;
    private readonly CreateCollectionCommand createCollectionCommand;
    private readonly AddCardToCollectionCommand addCardToCollectionCommand;

    public AddPublicCollectionCommand(
        ITransactionProvider transactionProvider,
        ICollectionQueryResolver collectionQueryResolver,
        ICardsQueryResolver cardsQueryResolver,
        IPublicCollectionQueryResolver publicCollectionQueryResolver,
        ICollectionPublicationQueryResolver collectionPublicationQueryResolver,
        IPublicCollectionSubscriberQueryResolver subscriberQueryResolver,
        ICollectionPublicationMutationResolver collectionPublicationMutationResolver,
        IPublicCollectionSubscriberMutationResolver publicCollectionSubscriberMutationResolver,
        CreateCollectionCommand createCollectionCommand,
        AddCardToCollectionCommand addCardToCollectionCommand)
    {
        this.transactionProvider = transactionProvider;
        this.collectionQueryResolver = collectionQueryResolver;
        this.cardsQueryResolver = cardsQueryResolver;
        this.publicCollectionQueryResolver = publicCollectionQueryResolver;
        this.collectionPublicationQueryResolver = collectionPublicationQueryResolver;
        this.subscriberQueryResolver = subscriberQueryResolver;
        this.collectionPublicationMutationResolver = collectionPublicationMutationResolver;
        this.publicCollectionSubscriberMutationResolver = publicCollectionSubscriberMutationResolver;
        this.createCollectionCommand = createCollectionCommand;
        this.addCardToCollectionCommand = addCardToCollectionCommand;
    }

    public async Task<Result<Collection>> Handle(AddPublicCollectionRequest request)
    {
        var (publicCollectionUserId, publicCollectionId, myUserId, myCollectionId, newCollectionName, checkUnique) = request;

        if (myCollectionId == null && string.IsNullOrEmpty(newCollectionName))
            return new BadRequestError();

        var publicCollection = await publicCollectionQueryResolver.Find(publicCollectionUserId, publicCollectionId);

        if (publicCollection == null)
        {
            return new NotFoundError("Public collection");
        }

        using var transaction = transactionProvider.CreateScope();

        Collection myCollection;

        if (myCollectionId != null)
        {
            var foundCollection = await collectionQueryResolver.Find(myUserId, myCollectionId);

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

        var publicCards = await cardsQueryResolver.GetAll(publicCollectionUserId, publicCollectionId);

        if (publicCards.Count == 0)
        {
            return new BadRequestError("No cards in the public collection");
        }

        var myCards = checkUnique 
            ? await cardsQueryResolver.GetAll(myUserId, myCollection.Id) 
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

        var publication = await collectionPublicationQueryResolver.Find(publicCollectionUserId, publicCollectionId);

        if (publication == null)
        {
            Debug.Fail("publication == null");
            return new InternalError();
        }

        var subscriber = await subscriberQueryResolver.Find(publicCollectionUserId, publicCollectionId, myUserId);

        if (subscriber == null)
        {
            var addSubscriberResult = publicCollectionSubscriberMutationResolver.Add(new PublicCollectionSubscriber()
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

            subscriber = addSubscriberResult.Value;
        }
        else
        {
            subscriber.IsAdded = true;
            var updateSubscriberResult = publicCollectionSubscriberMutationResolver.Update(subscriber);

            if (updateSubscriberResult.IsFailed)
                return new InternalError();
        }
        
        publication.SubscribersCount++;
        var updatePublicationResult = collectionPublicationMutationResolver.Update(publication);

        if (updatePublicationResult.IsFailed)
        {
            return new InternalError();
        }

        transaction.Complete();
        return myCollection;
    }
}