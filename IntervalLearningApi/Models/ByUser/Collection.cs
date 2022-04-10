using Newtonsoft.Json;
using NodaTime;

namespace IntervalLearningApi.Models.ByUser;

public class Collection
{
    [JsonProperty("userId")]
    public string ParentUserId { get; }
    public short Id { get; }
    public string Title { get; }
    public Instant CreatedAt { get; }
    public long DefaultScheduleUserId { get; }
    public short DefaultScheduleId { get; }
    public short ThemeId { get; }

    public short CardsCount { get; }
    public short StartedCards { get; }
    public short FinishedCards { get; }

    public Collection(
        string parentUserId, short id, string title, Instant createdAt,
        long defaultScheduleUserId, short defaultScheduleId, short themeId, 
        short cardsCount, short startedCards, short finishedCards)
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
        DefaultScheduleUserId = defaultScheduleUserId;
    }
}

public class CreateCollectionItem
{
    public long ScheduleUserId { get; set; }
    public short ScheduleId { get; set; }
    public short ThemeId { get; set; }
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
//        int cardsCount) 
//        : base(
//            parentUserId, 
//            id, 
//            title, 
//            createdAt, 
//            defaultScheduleUserId, 
//            defaultScheduleId, 
//            themeId, 
//            cardsCount)
//    {
//    }
//}