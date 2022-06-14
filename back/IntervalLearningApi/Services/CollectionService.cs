using System.Diagnostics;
using DB;
using DB.Models;
using DB.Models.Dictionary;
using DB.Models.Store;
using Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace IntervalLearningApi.Services;

public class CollectionService
{
    private readonly ApplicationContext db;
    private readonly CardsService cardsService;
    private readonly UserMetadataService metadataService;

    public CollectionService(
        ApplicationContext db,
        CardsService cardsService,
        UserMetadataService metadataService)
    {
        this.db = db;
        this.cardsService = cardsService;
        this.metadataService = metadataService;
    }

    public Task<CollectionEntity?> Find(long userId, short collectionId)
    {
        return db.Collections
            .Include(c => c.CollectionPublicationEntity)
            .SingleOrDefaultAsync(c => c.ParentUserId == userId && c.Id == collectionId);
    }

    public Task<List<CollectionEntity>> GetAllByUserId(long userId)
    {
        var collections = db.Collections
            .Where(c => c.ParentUserId == userId)
            .Include(c => c.CollectionPublicationEntity)
            .ToListAsync();

        return collections;
    }

    public async Task<Dictionary<DateTime, List<RepeatingPhase>>> GetRepeatCollections(long userId)
    {
        var queueItems = await db.Queue
            .Where(q => q.ParentUserId == userId)
            .Include(q => q.ParentRepeatsSchedule).ThenInclude(q => q.Phases)
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
            var phase = schedule.Phases.OrderBy(p => p.Id).Skip(queueItem.PhaseIndex).First();

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
                    phase.Description);

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

    public async Task<(int totalCollections, List<CollectionEntity> canStartCollections)> GetCanStart(
        long userId,
        long scheduleUserId,
        short scheduleId,
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
            collection.NotStartedCardsCount = (short)notStartedCards;
        }

        return (totalCollections, canStartCollections);
    }

    public class CreateOrPatchCollection : ICreateOrEditModel
    {
        public long ParentUserId { get; }
        public short ThemeId { get; }
        public string Title { get;  }
        public bool IsDefaultBackSide { get; }

        public CreateOrPatchCollection(
            long parentUserId, 
            string title, 
            bool isDefaultBackSide,
            short themeId)
        {
            ParentUserId = parentUserId;
            Title = TextMaster.RemoveWhiteSpaces(title, true);
            IsDefaultBackSide = isDefaultBackSide;
            ThemeId = themeId;
        }
    }

    public (CollectionEntity? collection, string? error) CreateOrEdit(CreateOrPatchCollection item, short? collectionId)
    {
        var collection = collectionId == null
            ? new CollectionEntity()
            : db.Collections.Find(item.ParentUserId, collectionId);

        if (collection == null)
            return (null, "Collection not found");

        var entry = db.Entry(collection);
        entry.CurrentValues.SetValues(item);

        try
        {
            if (collectionId == null)
                entry.State = EntityState.Added;

            db.SaveChanges();
            return (collection, null);
        }
        catch
        {
            return (null, "Unknown error");
        }
    }

    public (CardEntity? card, string? error) CreateOrEditCard(long userId,
        short collectionId,
        short? cardId,
        string frontText,
        string promptText,
        string backText,
        string? description,
        List<string>? examples,
        bool disableTransaction = false)
    {
        var collection = db.Collections.Find(userId, collectionId);

        if (collection == null)
            return (null, "Collection not found");

        if (!disableTransaction)
            db.Database.BeginTransaction();

        var (card, error, isCreated) = cardsService.CreateOrEdit(
            new CardsService.CreateOrPatchCard(
                userId,
                collectionId,
                frontText,
                promptText,
                backText,
                description,
                examples),
            cardId);

        if (error != null)
        {
            if (!disableTransaction)
                db.Database.RollbackTransaction();
            return (card, error);
        }

        if (isCreated)
        {
            collection.CardsCount++;
        }

        db.SaveChanges();

        if (!disableTransaction)
            db.Database.CommitTransaction();

        return (card, null);
    }

    public async Task<(List<WordEntity>? words, LanguageEntity? language, string? error)> GetRandomWords(
        long userId, 
        short collectionId)
    {
        var collection = await db.Collections.FindAsync(userId, collectionId);

        if (collection == null)
        {
            return (null, null, "Collection not found");
        }

        var theme = await db.Themes.FindAsync(collection.ThemeId);

        if (theme?.LanguageId == null)
        {
            return (null, null, theme == null ? "Theme not found" : "Language not linked");
        }

        var language = await db.Languages.FindAsync(theme.LanguageId);

        if (language == null)
            return (null, null, "Language not found");

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
                .Exists(c => string.Equals(c.FrontSideText, w.Word, StringComparison.InvariantCultureIgnoreCase)))
            .Take(30)
            .ToList();

        return (resultWords, language, null);
    }

    public async Task<(CollectionEntity? collection, string? error)> MakePublic(long userId, short collectionId)
    {
        var collection = await db.Collections.FindAsync(userId, collectionId);

        if (collection == null)
            return (null, "Not found");

        await db.Database.BeginTransactionAsync();

        var publication = db.CollectionPublications.Find(userId, collectionId)
            ?? db.CreateByProperties<CollectionPublicationEntity>(new CreateCollectionPublication(userId, collectionId));

        var isPublished = db.SoftSaveChanges();

        if (!isPublished)
        {
            await db.Database.RollbackTransactionAsync();
            return (null, "unable to create publication");
        }

        collection.IsPublic = true;

        var isOk = db.SoftSaveChanges();

        if (!isOk)
        {
            await db.Database.RollbackTransactionAsync();
            return (null, "Unknown error");
        }

        await db.Database.CommitTransactionAsync();
        return (collection, null);
    }

    public async Task<(CollectionEntity? collection, string? error)> AddCardsToMyCollection(
        long publicCollectionUserId,
        short publicCollectionId,
        long myUserId,
        short? myCollectionId, 
        string? newCollectionName,
        bool checkUnique)
    {
        await using var transaction = await db.Database.BeginTransactionAsync();

        if (myCollectionId == null && string.IsNullOrEmpty(newCollectionName))
            return (null, "Bad request");

        var publicCollection = await db.Collections.FindAsync(publicCollectionUserId, publicCollectionId);

        if (publicCollection is not {IsPublic: true})
        {
            return (null, "public collection not found");
        }

        var myCollection = myCollectionId != null
            ? await db.Collections.FindAsync(myUserId, myCollectionId.Value)
            : db.CreateByProperties<CollectionEntity>(new CreateOrPatchCollection(
                myUserId,
                newCollectionName,
                false,
                publicCollection.ThemeId));

        if (myCollectionId == null && myCollection != null)
        {
            var isCreatedNew = db.SoftSaveChanges();

            if (!isCreatedNew)
            {
                return (null, "unable to create collection");
            }
        }

        if (myCollection == null)
        {
            return (null, "my collection not found");
        }

        if (publicCollection.ThemeId != myCollection.ThemeId)
        {
            return (null, "themes of collections are different");
        }

        var publicCards = await cardsService.GetAllCards(publicCollectionUserId, publicCollectionId);

        if (publicCards.Count == 0)
        {
            return (myCollection, null);
        }

        var myCards = checkUnique ? await cardsService.GetAllCards(myUserId, myCollection.Id) : new List<CardEntity>();
        var myCardsSet = new HashSet<string>(myCards.Select(c => c.FrontSideText));

        foreach (var publicCard in publicCards)
        {
            if (checkUnique && myCardsSet.Contains(publicCard.FrontSideText))
            {
                continue;
            }

            var (card, addCardError) = CreateOrEditCard(
                myUserId,
                myCollection.Id,
                null,
                publicCard.FrontSideText,
                publicCard.PromptText,
                publicCard.BackSideText,
                publicCard.Description,
                publicCard.Examples,
                true);

            if (card == null)
            {
                return (null, "error to add card. " + addCardError);
            }
        }

        var isOk = db.SoftSaveChanges();

        if (!isOk)
        {
            return (null, "unknown error");
        }

        var publication = db.CollectionPublications.Find(publicCollectionUserId, publicCollectionId);

        if (publication == null)
        {
            Debug.Fail("publication == null");
            return (null, "system error");
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

        var isSubscriberCreated = db.SoftSaveChanges();

        if (!isSubscriberCreated)
        {
            return (null, "subscription error");
        }

        await transaction.CommitAsync();
        return (myCollection, null);
    }

    public async Task<List<(CollectionEntity collection, PublicCollectionSubscriber? subscriber)>> SearchPublicCollections(
        long myUserId,
        short themeId, 
        string searchName, 
        int page, 
        int count)
    {
        var theme = await db.Themes.FindAsync(themeId);

        if (theme == null)
        {
            return new List<(CollectionEntity, PublicCollectionSubscriber?)>();
        }

        var lowerSearchName = searchName.ToLowerInvariant();

        var toSkip = (page - 1) * count;

        var foundCollections = await db.Collections
            .Where(c => c.ThemeId == themeId && c.IsPublic && c.Title.ToLower().StartsWith(lowerSearchName))
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

    public class RepeatingPhase
    {
        public long ScheduleUserId { get; }
        public short ScheduleId { get;  }
        public short PhaseIndex { get;  }
        public uint SecondsFromLastPhase { get; }
        public string? Description { get; }

        public List<RepeatingCollection> RepeatingCollections { get; set; } = new();

        public RepeatingPhase(
            long scheduleUserId,
            short scheduleId,
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
        public CollectionEntity Collection { get; }

        public int CardsToRepeatCount { get; set; }

        public RepeatingCollection(CollectionEntity collection)
        {
            Collection = collection;
        }
    }

    public async Task<CollectionEntity?> FindPublicCollection(long userId, short collectionId)
    {
        var collection = await db.Collections
            .Include(c => c.CollectionPublicationEntity)
            .SingleOrDefaultAsync(c => c.ParentUserId == userId && c.Id == collectionId);

        return collection is not {IsPublic: true} 
            ? null 
            : collection;
    }
}