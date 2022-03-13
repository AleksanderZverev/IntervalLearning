using Newtonsoft.Json;
using NodaTime;

namespace IntervalLearningApi.Models.ByUser;

public class Card
{
    [JsonProperty("UserId")]
    public long ParentUserId { get; }
    [JsonProperty("CollectionId")]
    public short ParentCollectionId { get; }
    public short Id { get; }
    public string BackSideText { get; }
    public string FrontSideText { get; }
    public Instant CreatedDate { get; }
    public bool? IsFinished { get; }
    public string? Description { get; }
    public List<string>? Examples { get; }
    public List<Remember>? Remembers { get; }

    public Card(long parentUserId, short parentCollectionId, short id, string backSideText, string frontSideText,
        Instant createdDate, bool? isFinished, string? description, List<string>? examples, List<Remember>? remembers)
    {
        ParentUserId = parentUserId;
        ParentCollectionId = parentCollectionId;
        Id = id;
        BackSideText = backSideText;
        FrontSideText = frontSideText;
        CreatedDate = createdDate;
        IsFinished = isFinished;
        Description = description;
        Examples = examples;
        Remembers = remembers;
    }
}