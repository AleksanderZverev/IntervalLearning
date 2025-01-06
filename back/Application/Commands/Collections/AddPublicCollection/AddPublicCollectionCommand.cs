using System.Diagnostics;
using Application.Commands.Cards.CreateCard;
using Application.Commands.Collections.CreateCollection;
using Domain.Card;
using Domain.Collection;
using Domain.Collection.ValueObjects;
using Domain.Deprecated.DbModels;
using DomainServices.DB.Queries.Study;
using DomainServices.DB.Repositories.Store;
using DomainServices.DB.Transactions;
using FluentResults;
using GlobalTools.Errors;

namespace Application.Commands.Collections.AddPublicCollection;

public class AddPublicCollectionCommand : ICommand<AddPublicCollectionCommandRequest, Collection>
{
    private readonly ITransactionProvider transactionProvider;
    private readonly IStoreRepository storeRepository;
    private readonly CreateCollectionCommand createCollectionCommand;
    private readonly CreateCardCommand createCardCommand;
    private readonly IStudyQueryRepository studyQueryRepository;

    public AddPublicCollectionCommand(
        ITransactionProvider transactionProvider,
        CreateCollectionCommand createCollectionCommand,
        CreateCardCommand createCardCommand,
        IStudyQueryRepository studyQueryRepository,
        IStoreRepository storeRepository)
    {
        this.transactionProvider = transactionProvider;
        this.createCollectionCommand = createCollectionCommand;
        this.createCardCommand = createCardCommand;
        this.studyQueryRepository = studyQueryRepository;
        this.storeRepository = storeRepository;
    }

    public async Task<Result<Collection>> Handle(AddPublicCollectionCommandRequest request)
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
            var createdCollectionResult = await createCollectionCommand.Handle(new CreateCollectionCommandRequest()
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

            var addedCardResult = await createCardCommand.Handle(new CreateCardRequest()
            {
                ParentUserId = myUserId,
                ParentCollectionId = myCollection.Id,
                RememberingText = publicCard.RememberingText,
                PromptText = publicCard.PromptText,
                MeaningText = publicCard.MeaningText,
                Description = publicCard.Description,
                Examples = publicCard.Examples,
                Tags = publicCard.Tags,
            });

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
            storeRepository.Subscribers.Add(new PublicCollectionSubscriber()
            {
                ParentUserId = publicCollectionUserId,
                ParentCollectionId = publicCollectionId,
                SubscriberUserId = myUserId,
                IsAdded = true,
            });
        }
        else
        {
            subscriber.IsAdded = true;
            storeRepository.Subscribers.Update(subscriber);
        }
        
        publication.SubscribersCount++;
        storeRepository.Publications.Update(publication);

        var updatePublicationResult = await storeRepository.SaveChangesAsync();
        
        if (updatePublicationResult.IsFailed)
        {
            return new InternalError();
        }

        transaction.Complete();
        return myCollection;
    }
}