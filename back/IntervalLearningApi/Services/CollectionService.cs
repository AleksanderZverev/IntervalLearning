using System.Diagnostics;
using Application.Commands.Cards.DeleteCard;
using Application.Common.Interfaces.DB.Transactions;
using DB;
using DB.Configurations.Study;
using DB.Models.Dictionary;
using DB.Models.Store;
using DB.Models.ValueObjects;
using Domain.Card;
using Domain.Card.ValueObjects;
using Domain.Collection;
using Domain.Collection.ValueObjects;
using Domain.Common.ValueObjects;
using Domain.Language;
using Domain.User.ValueObjects;
using FluentResults;
using Infrastructure;
using Infrastructure.Errors;
using Microsoft.EntityFrameworkCore;

namespace IntervalLearningApi.Services;

public class CollectionService
{
    private readonly ApplicationContext db;
    private readonly ITransactionProvider transactionProvider;
    private readonly CardsService cardsService;
    private readonly DeleteCardCommand deleteCardCommand;

    public CollectionService(
        ApplicationContext db,
        ITransactionProvider transactionProvider,
        CardsService cardsService,
        DeleteCardCommand deleteCardCommand)
    {
        this.db = db;
        this.transactionProvider = transactionProvider;
        this.cardsService = cardsService;
        this.deleteCardCommand = deleteCardCommand;
    }

    public Task<Collection?> Find(UserId userId, CollectionId collectionId)
    {
        return db.Collections
            .Include(c => c.CollectionPublicationEntity)
            .SingleOrDefaultAsync(c => c.ParentUserId == userId && c.Id == collectionId);
    }

    public Task<List<Collection>> GetAllByUserId(UserId userId)
    {
        var collections = db.Collections
            .Where(c => c.ParentUserId == userId)
            .Include(c => c.CollectionPublicationEntity)
            .ToListAsync();

        return collections;
    }

    public async Task<Dictionary<DateTime, List<RepeatingPhase>>> GetRepeatCollections(UserId userId)
    {
        //GetAll
        var queueItems = await db.Queue
            .Where(q => q.ParentUserId == userId)
            .Include(q => q.ParentRepeatsSchedule)
            .ThenInclude(q => q.Phases)
            .AsSplitQuery()
            .ToListAsync();

        var collectionIds = queueItems
            .Select(q => q.ParentCollectionId)
            .Distinct()
            .ToList();

        var collectionIdToCollection = await db.Collections
            .Where(c => c.ParentUserId == userId && collectionIds.Contains(c.Id))
            .ToDictionaryAsync(c => c.Id);

        var result = new Dictionary<DateTime, List<RepeatingPhase>>();

        foreach (var queueItem in queueItems)
        {
            var date = queueItem.Date.Date;
            var schedule = queueItem.ParentRepeatsSchedule;
            var phase = schedule.GetPhase(queueItem.PhaseIndex);

            if (!result.ContainsKey(date))
            {
                result.Add(date, new List<RepeatingPhase>());
            }

            var repeatingPhasesList = result[date];

            var repeatingPhase = repeatingPhasesList.SingleOrDefault(r =>
                r.ScheduleUserId == queueItem.ParentRepeatsScheduleUserId
                && r.ScheduleId == queueItem.ParentRepeatsScheduleId
                && r.PhaseIndex == queueItem.PhaseIndex);

            if (repeatingPhase == null)
            {
                repeatingPhase = new RepeatingPhase(
                    queueItem.ParentRepeatsScheduleUserId,
                    queueItem.ParentRepeatsScheduleId,
                    queueItem.PhaseIndex,
                    phase.SecondsFromLastPhase,
                    phase.OnLearnDescription);

                repeatingPhasesList.Add(repeatingPhase);
            }

            var collection = collectionIdToCollection[queueItem.ParentCollectionId];

            var repeatingCollection =
                repeatingPhase.RepeatingCollections.SingleOrDefault(q =>
                    q.Collection.Id == queueItem.ParentCollectionId);

            if (repeatingCollection == null)
            {
                repeatingCollection = new RepeatingCollection(collection);
                repeatingPhase.RepeatingCollections.Add(repeatingCollection);
            }

            repeatingCollection.CardsToRepeatCount++;
        }

        return result;
    }

    public async Task<(int totalCollections, List<Collection> canStartCollections)> GetCanStart(
        UserId userId,
        UserId scheduleUserId,
        ScheduleId scheduleId,
        int page = 1,
        int count = 30)
    {
        //TODO: can be slow.

        var canStartCards = await db.Cards
            .Where(c => c.ParentUserId == userId
                        && !db.Remembers.Any(r =>
                            r.ParentUserId == userId
                            && r.ParentCollectionId == c.ParentCollectionId
                            && r.ParentCardId == c.Id
                            && r.ParentRepeatsScheduleUserId == scheduleUserId
                            && r.ParentRepeatsScheduleId == scheduleId))
            .ToListAsync();
        
        var skip = (page - 1) * count;

        var canStartAllCollectionsIds = canStartCards
            .Select(c => c.ParentCollectionId)
            .Distinct()
            .ToList();

        var totalCollections = canStartAllCollectionsIds.Count;

        var collectionIdsToStart = canStartAllCollectionsIds.Skip(skip).Take(count).ToList();

        var canStartCollections = await db.Collections
            .Where(c => c.ParentUserId == userId && collectionIdsToStart.Contains(c.Id))
            .ToListAsync();

        var collectionToCardsCount = canStartCollections
            .GroupBy(c => c.Id)
            .ToDictionary(c => c.Key, c => canStartCards.Count(card => card.ParentCollectionId == c.Key));

        foreach (var collection in canStartCollections)
        {
            var notStartedCards = collectionToCardsCount[collection.Id];
            collection.NotStartedCardsCount = Counter.Create(notStartedCards).Value;
        }

        return (totalCollections, canStartCollections);
    }

    public class CreateOrPatchCollection
    {
        public required UserId ParentUserId { get; init; }
        public required ThemeId ThemeId { get; init; }
        public required ThemeTitle Title { get; init; }
        public bool IsDefaultBackSide { get; init; }
    }

    public Result<Collection> CreateOrEdit(
        CreateOrPatchCollection item, 
        CollectionId? collectionId)
    {
        return collectionId == null 
            ? Create(item) 
            : Edit(item, collectionId);
    }

    private Result<Collection> Edit(CreateOrPatchCollection item, CollectionId collectionId)
    {
        var collection = db.Collections.Find(item.ParentUserId, collectionId);

        if (collection == null)
            return new Error("Collection not found");
        
        collection.Title = CollectionTitle.Create(item.Title).Value;
        collection.ThemeId = item.ThemeId;
        collection.IsDefaultBackSide = item.IsDefaultBackSide;

        return db.SoftSaveChanges()
            ? collection
            : new Error("Internal error");
    }

    private Result<Collection> Create(CreateOrPatchCollection item)
    {
        var sequenceName = CollectionConfiguration.GetSequenceName(item.ParentUserId);
        
        db.EnsureSequenceCreated(sequenceName);
        var collectionNextId = db.GetSequenceNextValue16(sequenceName);
        var collectionId = CollectionId.Create(collectionNextId).Value;

        var newCollectionResult = Collection.Create(
            item.ParentUserId,
            collectionId,
            CollectionTitle.Create(item.Title).Value,
            item.ThemeId);

        if (newCollectionResult.IsFailed)
            return new Error("Creation error");

        var newCollection = newCollectionResult.Value;
        newCollection.IsDefaultBackSide = item.IsDefaultBackSide;
        
        db.Add(newCollection);

        return db.SoftSaveChanges()
            ? newCollection
            : new Error("Internal error");
    }

    public Result<Card> CreateOrEditCard(
        UserId userId,
        CollectionId collectionId,
        CardId? cardId,
        CardText frontText,
        CardText? promptText,
        CardText backText,
        CardDescription? description,
        List<CardExample> examples)
    {
        var collection = db.Collections.Find(userId, collectionId);

        if (collection == null)
            return new Error("Collection not found");

        var createOrPatchCard = new CardsService.CreateOrPatchCard
        {
            ParentUserId = userId,
            ParentCollectionId = collectionId,
            MeaningText = backText,
            RememberingText = frontText,
            PromptText = promptText,
            Description = description,
            Examples = examples
        };

        using var transaction = transactionProvider.CreateScope();
        
        var isCreation = cardId == null; 
        var cardResult = isCreation
            ? cardsService.Create(createOrPatchCard)
            : cardsService.Edit(createOrPatchCard, cardId);

        if (cardResult.IsFailed)
        {
            return cardResult;
        }

        if (isCreation)
        {
            collection.CardsCount.Increment();
            db.Update(collection);
        }

        if (!db.SoftSaveChanges())
        {
            return new InternalError();
        }

        transaction.Complete();
        return cardResult.Value;
    }
    
    public async Task<Result<Card>> DeleteCard(
        UserId userId,
        CollectionId collectionId,
        CardId cardId)
    {
        var collection = await db.Collections.FindAsync(userId, collectionId);

        if (collection == null)
            return new NotFoundError("Collection");

        using var transaction = transactionProvider.CreateScope();
        
        var deletionResult = await deleteCardCommand.Handle(new DeleteCardRequest(userId, collectionId, cardId));
        
        if (deletionResult.IsFailed)
        {
            return deletionResult;
        }

        collection.CardsCount.Decrement();
        db.Update(collection);

        if (!await db.SoftSaveChangesAsync())
        {
            return new InternalError();
        }

        transaction.Complete();
        return deletionResult.Value;
    }

    public async Task<Result<Card>> MoveCard(
        UserId userId,
        CollectionId sourceCollectionId,
        CollectionId destinationCollectionId,
        CardId cardId)
    {
        var sourceCollection = await db.Collections.FindAsync(userId, sourceCollectionId);
        var destinationCollection = await db.Collections.FindAsync(userId, destinationCollectionId);

        if (sourceCollection == null)
            return new NotFoundError("Source collection");
        if (destinationCollection == null)
            return new NotFoundError("Destination collection");

        using var transaction = transactionProvider.CreateScope();

        var movingResult = await cardsService.MoveCard(
            userId,
            sourceCollectionId,
            destinationCollectionId,
            cardId);

        if (movingResult.IsFailed)
        {
            return movingResult;
        }

        sourceCollection.CardsCount.Decrement();
        destinationCollection.CardsCount.Increment();

        if (!await db.SoftSaveChangesAsync())
        {
            //"Unable to increase cards count"
            return new InternalError();
        }

        transaction.Complete();
        return movingResult.Value;
    }

    public async Task<Result<(List<LanguageWord> words, Language language)>> GetRandomWords(
        UserId userId, 
        CollectionId collectionId)
    {
        var collection = await db.Collections.FindAsync(userId, collectionId);

        if (collection == null)
        {
            return new NotFoundError("Collection");
        }

        var theme = await db.Themes.FindAsync(collection.ThemeId);

        if (theme?.LanguageId == null)
        {
            return new NotFoundError(theme == null ? "Theme not found" : "Language not linked");
        }

        var language = await db.Languages.FindAsync(theme.LanguageId);

        if (language == null)
            return new NotFoundError("Language");

        var words = await db.Words.Where(w => w.LanguageId == theme.LanguageId).ToListAsync();
        words.Shuffle();

        var collectionIds = await db.Collections
            .Where(c => c.ParentUserId == userId && c.ThemeId == theme.Id)
            .Select(c => c.Id)
            .ToListAsync();

        var cards = await db.Cards
            .Where(c => c.ParentUserId == userId && collectionIds.Contains(c.ParentCollectionId))
            .ToListAsync();

        var resultWords = words
            .Where(w => !cards
                .Exists(c => string.Equals(c.RememberingText, w.Word, StringComparison.InvariantCultureIgnoreCase)))
            .Take(30)
            .ToList();

        return (resultWords, language);
    }

    public async Task<Result<Collection>> MakePublic(UserId userId, CollectionId collectionId)
    {
        var collection = await db.Collections.FindAsync(userId, collectionId);

        if (collection == null)
            return new NotFoundError("Collection");

        using var transaction = transactionProvider.CreateScope();

        var publication = db.CollectionPublications.Find(userId, collectionId)
            ?? db.CreateByProperties<CollectionPublicationEntity>(new CreateCollectionPublication(userId, collectionId));
        

        if (!db.SoftSaveChanges())
        {
            return new InternalError();
        }

        collection.MakePublic();
        
        if (!db.SoftSaveChanges())
        {
            return new InternalError();
        }

        transaction.Complete();
        return collection;
    }

    public async Task<Result<Collection>> AddCardsToMyCollection(
        UserId publicCollectionUserId,
        CollectionId publicCollectionId,
        UserId myUserId,
        CollectionId? myCollectionId, 
        string? newCollectionName,
        bool checkUnique)
    {
        using var transaction = transactionProvider.CreateScope();

        if (myCollectionId == null && string.IsNullOrEmpty(newCollectionName))
            return new BadRequestError();

        var publicCollection = await db.Collections.FindAsync(publicCollectionUserId, publicCollectionId);

        if (publicCollection is not {IsPublic: true})
        {
            return new NotFoundError("public collection");
        }

        var myCollection = myCollectionId != null
            ? await db.Collections.FindAsync(myUserId, myCollectionId.Value)
            : throw new NotImplementedException();
            // : db.CreateByProperties<Collection>(new CreateOrPatchCollection(
            //     myUserId,
            //     newCollectionName,
            //     false,
            //     publicCollection.ThemeId));

        // if (myCollectionId == null && myCollection != null)
        // {
        //     var isCreatedNew = db.SoftSaveChanges();
        //
        //     if (!isCreatedNew)
        //     {
        //         return (null, "unable to create collection");
        //     }
        // }

        if (myCollection == null)
        {
            return new NotFoundError("collection");
        }

        if (publicCollection.ThemeId != myCollection.ThemeId)
        {
            return new BadRequestError("Themes of collections are different");
        }

        var publicCards = await cardsService.GetAllCards(publicCollectionUserId, publicCollectionId);

        if (publicCards.Count == 0)
        {
            return myCollection;
        }

        var myCards = checkUnique ?
            await cardsService.GetAllCards(myUserId, myCollection.Id) 
            : new List<Card>();
        
        var myCardsSet = new HashSet<string>(myCards.Select(c => c.RememberingText.Value));

        foreach (var publicCard in publicCards)
        {
            if (checkUnique && myCardsSet.Contains(publicCard.RememberingText))
            {
                continue;
            }

            var cardResult = CreateOrEditCard(
                myUserId,
                myCollection.Id,
                null,
                publicCard.RememberingText,
                publicCard.PromptText,
                publicCard.MeaningText,
                publicCard.Description,
                publicCard.Examples);

            if (cardResult.IsFailed)
            {
                return cardResult.ToResult();
            }
        }

        if (!db.SoftSaveChanges())
        {
            return new InternalError();
        }

        var publication = db.CollectionPublications.Find(publicCollectionUserId, publicCollectionId);

        if (publication == null)
        {
            Debug.Fail("publication == null");
            return new InternalError();
        }

        var subscriber = db.PublicCollectionSubscribers.Find(publicCollectionUserId, publicCollectionId, myUserId);

        if (subscriber == null)
        {
            subscriber = db.CreateByProperties<PublicCollectionSubscriber>(new CreatePublicCollectionSubscriber(
                publicCollectionUserId,
                publicCollectionId,
                myUserId));

            publication.SubscribersCount++;
        }

        subscriber.IsAdded = true;


        if (!db.SoftSaveChanges())
        {
            return new InternalError();
        }

        transaction.Complete();
        return myCollection;
    }

    public async Task<List<(Collection collection, PublicCollectionSubscriber? subscriber)>> SearchPublicCollections(
        UserId myUserId,
        ThemeId themeId, 
        string searchName, 
        int page, 
        int count)
    {
        var theme = await db.Themes.FindAsync(themeId);

        if (theme == null)
        {
            return new List<(Collection, PublicCollectionSubscriber?)>();
        }

        var lowerSearchName = searchName.ToLowerInvariant();

        var toSkip = (page - 1) * count;

        var foundCollections = await db.Collections
            .Where(c => c.ThemeId == themeId && c.IsPublic && EF.Functions.ILike(c.Title, $"{lowerSearchName}%"))
            .Include(c => c.CollectionPublicationEntity)
            .Include(c => c.ParentUser)
            .AsSplitQuery()
            .ToListAsync();

        var targetCollections = foundCollections
            //.Where(c => c.IsPublic)
            .Skip(toSkip)
            .Take(count)
            .ToList();

        var result = targetCollections
            .Select(c =>
            {
                var subscription = db.PublicCollectionSubscribers.Find(c.ParentUserId, c.Id, myUserId);
                return (c, subscription);
            })
            .ToList();

        return result;
    }
    
    public async Task<List<Collection>> SearchCollections(
        UserId userId,
        ThemeId themeId, 
        string searchName, 
        int page, 
        int count)
    {
        var theme = await db.Themes.FindAsync(themeId);

        if (theme == null)
            return new List<Collection>();

        var lowerSearchName = searchName.ToLowerInvariant();

        var toSkip = (page - 1) * count;

        return await db.Collections
            .Where(c => 
                c.ParentUserId == userId
                && c.ThemeId == themeId
                && EF.Functions.ILike(c.Title, $"{lowerSearchName}%"))
            .Skip(toSkip)
            .Take(count)
            .ToListAsync();
    }

    public class RepeatingPhase
    {
        public UserId ScheduleUserId { get; }
        public ScheduleId ScheduleId { get;  }
        public short PhaseIndex { get;  }
        public uint SecondsFromLastPhase { get; }
        public string? Description { get; }

        public List<RepeatingCollection> RepeatingCollections { get; set; } = new();

        public RepeatingPhase(
            UserId scheduleUserId,
            ScheduleId scheduleId,
            short phaseIndex,
            uint secondsFromLastPhase,
            string? description)
        {
            ScheduleUserId = scheduleUserId;
            ScheduleId = scheduleId;
            PhaseIndex = phaseIndex;
            SecondsFromLastPhase = secondsFromLastPhase;
            Description = description;
        }
    }

    public class RepeatingCollection
    {
        public Collection Collection { get; }

        public int CardsToRepeatCount { get; set; }

        public RepeatingCollection(Collection collection)
        {
            Collection = collection;
        }
    }

    public async Task<Collection?> FindPublicCollection(UserId userId, CollectionId collectionId)
    {
        var collection = await db.Collections
            .Include(c => c.CollectionPublicationEntity)
            .SingleOrDefaultAsync(c => c.ParentUserId == userId && c.Id == collectionId);

        return collection is not {IsPublic: true} 
            ? null 
            : collection;
    }
}