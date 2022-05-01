using Newtonsoft.Json;

namespace IntervalLearningApi.Models.ByUser;

public class Card
{
    [JsonProperty("userId")]
    public string ParentUserId { get; }
    [JsonProperty("collectionId")]
    public short ParentCollectionId { get; }
    public short Id { get; }
    public string BackSideText { get; }
    public string FrontSideText { get; }
    public DateTime CreatedDate { get; }
    public string? Description { get; }
    public List<string>? Examples { get; }
    public List<Remember>? Remembers { get; }

    public Card(
        string parentUserId,
        short parentCollectionId,
        short id,
        string backSideText,
        string frontSideText,
        DateTime createdDate,
        string? description,
        List<string>? examples,
        List<Remember>? remembers)
    {
        ParentUserId = parentUserId;
        ParentCollectionId = parentCollectionId;
        Id = id;
        BackSideText = backSideText;
        FrontSideText = frontSideText;
        CreatedDate = createdDate;
        Description = description;
        Examples = examples;
        Remembers = remembers;
    }
}