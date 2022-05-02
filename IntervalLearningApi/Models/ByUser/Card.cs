using Newtonsoft.Json;

namespace IntervalLearningApi.Models.ByUser;

public class Card
{
    [JsonProperty("userId")]
    public string ParentUserId { get; }
    [JsonProperty("collectionId")]
    public string ParentCollectionId { get; }
    public string Id { get; }
    public string BackSideText { get; }
    public string FrontSideText { get; }
    public DateTime CreatedDate { get; }
    public string? Description { get; }
    public List<string>? Examples { get; }
    public List<Remember>? Remembers { get; }

    public Card(
        long parentUserId,
        short parentCollectionId,
        short id,
        string backSideText,
        string frontSideText,
        DateTime createdDate,
        string? description,
        List<string>? examples,
        List<Remember>? remembers)
    {
        ParentUserId = parentUserId.ToString();
        ParentCollectionId = parentCollectionId.ToString();
        Id = id.ToString();
        BackSideText = backSideText;
        FrontSideText = frontSideText;
        CreatedDate = createdDate;
        Description = description;
        Examples = examples;
        Remembers = remembers;
    }
}