using System.ComponentModel.DataAnnotations;
using DB.Models.ValueObjects;
using IntervalLearningApi.Models.RepeatsSchedule;
using IntervalLearningApi.Services;
using Mapster;

namespace IntervalLearningApi.Models.ByUser;

public class CreteScheduleRegister : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<short, ScheduleId>()
            .Map(d => d, s => ScheduleId.Create(s).Value);
        
        config.NewConfig<string, ScheduleTitle>()
            .Map(d => d, s => ScheduleTitle.Create(s).Value);
        
        config.NewConfig<string, ScheduleShortDescription>()
            .Map(d => d, s => ScheduleShortDescription.Create(s).Value);
        
        config.NewConfig<string, ScheduleLongDescription>()
            .Map(d => d, s => ScheduleLongDescription.Create(s).Value);

        config.NewConfig<CreateScheduleRequest, CreateScheduleItem>()
            .Map(d => d.OnStartLearningDescription, s => s.Description);

        //Update Request

        config.NewConfig<UpdateScheduleRequest, PatchRepeatsSchedule>();

        config.NewConfig<UpdatePhaseInfo, PhaseInfo>();
    }
}

public class CreateScheduleRequest
{
    [Required]
    public short CardsCountPerPhase { get; set; }
    [Required]
    public int ForgottenBehavior { get; set; }
    [Required]
    public string Title { get; set; }

    [StringLength(200)]
    public string? ShortDescription { get; set; }

    [StringLength(1000)]
    public string? Description { get; set; }
    [Required]
    public List<PhaseInfo> Phases { get; set; }

    [StringLength(200)]
    public string? DefaultPhaseShortDescription { get; set; }
    [StringLength(1000)]
    public string? DefaultPhaseDescription { get; set; }
    [StringLength(200)]
    public string? DefaultRepeatPhaseShortDescription { get; set; }
    [StringLength(1000)]
    public string? DefaultRepeatPhaseDescription { get; set; }
}

public class UpdateScheduleRequest
{
    [Required]
    public short CardsCountPerPhase { get; set; }
    [Required]
    public string Title { get; set; }

    [StringLength(200)]
    public string? ShortDescription { get; set; }

    [StringLength(1000)]
    public string? Description { get; set; }

    public List<UpdatePhaseInfo>? Phases { get; set; }

    [StringLength(200)]
    public string? DefaultPhaseShortDescription { get; set; }
    [StringLength(1000)]
    public string? DefaultPhaseDescription { get; set; }
    [StringLength(200)]
    public string? DefaultRepeatPhaseShortDescription { get; set; }
    [StringLength(1000)]
    public string? DefaultRepeatPhaseDescription { get; set; }
}