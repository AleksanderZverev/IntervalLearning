using Newtonsoft.Json;
using NodaTime;

namespace IntervalLearningApi.Models.ByUser;

public class Remember
{
    [JsonProperty("userId")]
    public string ParentUserId { get; }
    [JsonProperty("collectionId")]
    public string ParentCollectionId { get; }
    [JsonProperty("cardId")]
    public string ParentCardId { get; }
    public string Id { get; }
    public float Weight { get; }
    public short PhaseIndex { get; }
    public DateTime RepeatedDate { get; }

    public Remember(
        long parentUserId,
        short parentCollectionId,
        short parentCardId,
        short id,
        float weight,
        short phaseIndex,
        DateTime repeatedDate)
    {
        ParentUserId = parentUserId.ToString();
        ParentCollectionId = parentCollectionId.ToString();
        ParentCardId = parentCardId.ToString();
        Id = id.ToString();
        Weight = weight;
        PhaseIndex = phaseIndex;
        RepeatedDate = repeatedDate;
    }
}