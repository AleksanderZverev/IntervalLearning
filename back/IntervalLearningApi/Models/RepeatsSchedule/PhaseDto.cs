using DB.Models;
using Mapster;
using Newtonsoft.Json;

namespace IntervalLearningApi.Models.RepeatsSchedule;

public class PhaseRegister : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<PhaseEntity, PhaseDto>()
            .Map(d => d.Description, s => s.OnLearnDescription);
    }
}

public class PhaseDto
{
    [JsonProperty("userId")]
    public string ParentUserId { get; set; }

    [JsonProperty("scheduleId")]
    public string ParentRepeatsScheduleId { get; set; }
    public string Id { get; set; }
    public uint SecondsFromLastPhase { get; set; }
    public string? ShortDescription { get; set; }
    public string? Description { get; set; }
    public bool IsDefaultValueSide { get; set; }
}