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
            .MapWith(id => ScheduleId.Create(id).Value);

        config.NewConfig<string, ScheduleTitle>()
            .MapWith(s => ScheduleTitle.Create(s).Value);

        config.NewConfig<string, LongSingleLineString>()
            .MapWith(s => LongSingleLineString.Create(s).Value);
        
        config.NewConfig<string, LongMultiLineString>()
            .MapWith(s => LongMultiLineString.Create(s).Value);

        
        config.NewConfig<CreateScheduleRequest, CreateScheduleItem>()
            .Map(d => d.OnStartLearningDescription, s => s.Description)
            .IgnoreNullValues(true);
        

        config.NewConfig<UpdateScheduleRequest, UpdateScheduleItem>()
            .Map(d => d.OnStartLearningDescription, s => s.Description)
            .IgnoreNullValues(true);
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
    public List<CreatePhaseDto> Phases { get; set; }

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

    public List<UpdatePhaseDto>? Phases { get; set; }

    [StringLength(200)]
    public string? DefaultPhaseShortDescription { get; set; }
    [StringLength(1000)]
    public string? DefaultPhaseDescription { get; set; }
    [StringLength(200)]
    public string? DefaultRepeatPhaseShortDescription { get; set; }
    [StringLength(1000)]
    public string? DefaultRepeatPhaseDescription { get; set; }
}