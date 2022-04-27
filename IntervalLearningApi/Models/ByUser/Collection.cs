using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;
using NodaTime;

namespace IntervalLearningApi.Models.ByUser;

public class Collection
{
    [JsonProperty("userId")]
    public string ParentUserId { get; }
    public short Id { get; }
    public string Title { get; }
    public DateTime CreatedAt { get; }
    public long DefaultScheduleUserId { get; }
    public short DefaultScheduleId { get; }
    public short ThemeId { get; }

    public short CardsCount { get; }
    public short NotStartedCards { get; set; }
    public short StartedCards { get; }
    public short FinishedCards { get; }

    public Collection(
        string parentUserId, short id, string title, DateTime createdAt,
        long defaultScheduleUserId, short defaultScheduleId, short themeId, 
        short cardsCount, short startedCards, short finishedCards, short notStartedCards)
    {
        ParentUserId = parentUserId;
        Id = id;
        Title = title;
        CreatedAt = createdAt;
        DefaultScheduleId = defaultScheduleId;
        ThemeId = themeId;
        CardsCount = cardsCount;
        StartedCards = startedCards;
        FinishedCards = finishedCards;
        NotStartedCards = notStartedCards;
        DefaultScheduleUserId = defaultScheduleUserId;
    }
}

public class CreateCollectionItem
{
    [Required]
    public long ScheduleUserId { get; set; }
    [Required]
    public short ScheduleId { get; set; }
    [Required]
    public short ThemeId { get; set; }
    [Required]
    [StringLength(100)]
    public string Title { get; set; }
    public bool IsDefaultBackSide { get; set; }
}

public class GetNotFinishedResponse
{
    public List<Collection> StartedCollections { get; }

    public List<Collection> NotStartedCollections { get; }

    public GetNotFinishedResponse(List<Collection> startedCollections, List<Collection> notStartedCollections)
    {
        StartedCollections = startedCollections;
        NotStartedCollections = notStartedCollections;
    }
}

public class QueueCollectionResponse
{
    public Dictionary<DateTime, List<QueueCollectionDto>> DateToCollectionsQueue { get; }

    public QueueCollectionResponse(Dictionary<DateTime, List<QueueCollectionDto>> dateToCollectionsQueue)
    {
        DateToCollectionsQueue = dateToCollectionsQueue;
    }
}

public class QueueCollectionDto
{
    public Collection Collection { get; }

    public int CardsToRepeatCount { get; }

    public QueueCollectionDto(Collection collection, int cardsToRepeatCount)
    {
        Collection = collection;
        CardsToRepeatCount = cardsToRepeatCount;
    }
}

//public class LearningCollection : Collection
//{
//    public LearningCollection(
//        string parentUserId, 
//        short id, 
//        string title, 
//        Instant createdAt, 
//        long defaultScheduleUserId, 
//        short defaultScheduleId, 
//        short themeId, 
//        int cardsToRepeatCount) 
//        : base(
//            parentUserId, 
//            id, 
//            title, 
//            createdAt, 
//            defaultScheduleUserId, 
//            defaultScheduleId, 
//            themeId, 
//            cardsToRepeatCount)
//    {
//    }
//}