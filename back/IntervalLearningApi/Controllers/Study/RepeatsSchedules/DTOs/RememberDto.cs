using Domain.Schedule.Entities.Remember;
using Domain.Schedule.Entities.Remember.ValueObjects;
using Mapster;
using Newtonsoft.Json;

namespace IntervalLearningApi.Controllers.Study.RepeatsSchedules.DTOs;

public class RememberRegister : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<RememberWeight, float>()
            .MapWith(r => r.Value);
        config.NewConfig<Remember, RememberDto>();
    }
}

public class RememberDto
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