using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using DB.Models;
using DB.Models.ValueObjects;
using IntervalLearningApi.Services;
using Mapster;
using Newtonsoft.Json;

namespace IntervalLearningApi.Models.RepeatsSchedule;

public class PhaseRegister : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<Phase, PhaseDto>()
            .Map(d => d.Description, s => s.OnLearnDescription);

        config.NewConfig<string, PhaseId>()
            .MapWith(id => PhaseId.Create(short.Parse(id)).Value);
        
        config.NewConfig<CreatePhaseDto, PhaseInfo>()
            .IgnoreNullValues(true);
        config.NewConfig<UpdatePhaseDto, UpdatePhaseInfo>()
            .IgnoreNullValues(true);
    }
}

public abstract class BasePhaseBody
{
    [Required]
    public string Id { get; set; }
    [Required]
    public uint SecondsFromLastPhase { get; set; }
    [StringLength(200)]
    public string? ShortDescription { get; set; }
    [StringLength(200)]
    public string? Description { get; set; }
    public bool IsDefaultValueSide { get; set; }
}

public class CreatePhaseDto : BasePhaseBody
{
}

public class UpdatePhaseDto : BasePhaseBody
{
}

public class PhaseDto : BasePhaseBody
{
    [JsonProperty("userId")]
    public string ParentUserId { get; set; }

    [JsonProperty("scheduleId")]
    public string ParentRepeatsScheduleId { get; set; }
}