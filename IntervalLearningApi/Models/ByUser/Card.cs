using Newtonsoft.Json;
using NodaTime;

namespace IntervalLearningApi.Models.ByUser;

public class Card
{
    [JsonProperty("userId")]
    public string ParentUserId { get; }
    [JsonProperty("collectionId")]
    public short ParentCollectionId { get; }
    public short Id { get; }
    public long ScheduleUserId { get; }
    public short ScheduleId { get; }
    public string BackSideText { get; }
    public string FrontSideText { get; }
    public Instant CreatedDate { get; }
    public bool? IsFinished { get; }
    public string? Description { get; }
    public List<string>? Examples { get; }
    public List<Remember>? Remembers { get; }

    public Card(
        string parentUserId, short parentCollectionId, short id, 
        long scheduleUserId, short scheduleId, 
        string backSideText, string frontSideText,
        Instant createdDate, bool? isFinished, string? description, List<string>? examples, List<Remember>? remembers)
    {
        ParentUserId = parentUserId;
        ParentCollectionId = parentCollectionId;
        Id = id;
        ScheduleUserId = scheduleUserId;
        ScheduleId = scheduleId;
        BackSideText = backSideText;
        FrontSideText = frontSideText;
        CreatedDate = createdDate;
        IsFinished = isFinished;
        Description = description;
        Examples = examples;
        Remembers = remembers;
    }
}