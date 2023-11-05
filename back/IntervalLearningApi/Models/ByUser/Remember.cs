using DB.Models;
using Mapster;
using Newtonsoft.Json;

namespace IntervalLearningApi.Models.ByUser;

public class RememberRegister : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<RememberEntity, Remember>();
    }
}

public class Remember
{
    [JsonProperty("userId")]
    public string ParentUserId { get; set; }
    [JsonProperty("collectionId")]
    public string ParentCollectionId { get; set; }
    [JsonProperty("cardId")]
    public string ParentCardId { get; set; }
    public string Id { get; set; }
    public float Weight { get; set; }
    public short PhaseIndex { get; set; }
    public DateTime RepeatedDate { get; set; }
}