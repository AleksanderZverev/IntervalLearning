using Newtonsoft.Json;
using NodaTime;

namespace IntervalLearningApi.Models.ByUser;

public class Remember
{
    [JsonProperty("userId")]
    public long ParentUserId { get; }
    [JsonProperty("collectionId")]
    public short ParentCollectionId { get; }
    [JsonProperty("cardId")]
    public short ParentCardId { get; }
    public short Id { get; }
    public float Weight { get; }
    public short PhaseStep { get; }
    public DateTime RepeatedDate { get; }

    public Remember(long parentUserId, short parentCollectionId, short parentCardId, short id, float weight,
        short phaseStep, DateTime repeatedDate)
    {
        ParentUserId = parentUserId;
        ParentCollectionId = parentCollectionId;
        ParentCardId = parentCardId;
        Id = id;
        Weight = weight;
        PhaseStep = phaseStep;
        RepeatedDate = repeatedDate;
    }
}