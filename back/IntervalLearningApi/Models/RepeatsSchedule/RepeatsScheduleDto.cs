using DB.Models;
using DB.Models.ValueObjects;
using Mapster;
using Newtonsoft.Json;

namespace IntervalLearningApi.Models.RepeatsSchedule;

public class ScheduleRegister : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<ScheduleId, short>()
            .Map(d => d, s => s.Value);
        
        config.NewConfig<DB.Models.RepeatsSchedule, RepeatsScheduleDto>()
            .Map(d => d.Description, s => s.OnStartLearningDescription);
    }
}

public class RepeatsScheduleDto
{
    [JsonProperty("userId")]
    public string ParentUserId { get; set; }
    public string Id { get; set; }
    public string Title { get; set; }
    public short CardsCountPerPhase { get; set; }
    public string? ShortDescription { get; set; }
    
    public string? Description { get; set; }
    public string? DefaultPhaseShortDescription { get; set; }
    public string? DefaultPhaseDescription { get; set; }
    public string? DefaultRepeatPhaseShortDescription { get; set; }
    public string? DefaultRepeatPhaseDescription { get; set; }
    public bool IsRecommended { get; set; }
    public int ForgottenBehavior { get; set; }
    public List<PhaseDto> Phases { get; set; }
}