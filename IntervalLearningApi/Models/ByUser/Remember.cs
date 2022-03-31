using Newtonsoft.Json;

namespace IntervalLearningApi.Models.ByUser;

public class Remember
{
    [JsonProperty("userId")]
    public long ParentUserId { get; }
    [JsonProperty("collectionId")]
    public short ParentCollectionId { get; }
    [JsonProperty("cardId")]
    public short ParentCardId { get; }
    public byte Id { get; }
    public float Weight { get; }
    public byte PhaseStep { get; }
    public int PassedSecondsFromLastStep { get; }

    public Remember(long parentUserId, short parentCollectionId, short parentCardId, byte id, float weight,
        byte phaseStep, int passedSecondsFromLastStep)
    {
        ParentUserId = parentUserId;
        ParentCollectionId = parentCollectionId;
        ParentCardId = parentCardId;
        Id = id;
        Weight = weight;
        PhaseStep = phaseStep;
        PassedSecondsFromLastStep = passedSecondsFromLastStep;
    }
}