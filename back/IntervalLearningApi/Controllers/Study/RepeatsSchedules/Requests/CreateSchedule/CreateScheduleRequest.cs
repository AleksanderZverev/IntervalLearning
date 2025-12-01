using System.ComponentModel.DataAnnotations;
using Application.Commands.Schedules.CreateSchedule;
using Application.Commands.Schedules.UpdateSchedule;
using Domain.Common.ValueObjects.Text.MultiLine;
using Domain.Common.ValueObjects.Text.SingleLine;
using Domain.Schedule.ValueObjects;
using FluentValidation;
using IntervalLearningApi.Controllers.Study.RepeatsSchedules.Requests.UpdateSchedule;
using IntervalLearningApi.Extensions;
using Mapster;

namespace IntervalLearningApi.Controllers.Study.RepeatsSchedules.Requests.CreateSchedule;

public class CreteScheduleRegister : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<short, ScheduleId>()
            .MapWith(id => ScheduleId.Create(id).Value);

        config.NewConfig<string, ScheduleTitle>()
            .MapWith(s => ScheduleTitle.Create(s).Value);

        config.NewConfig<string?, LongSingleLineString?>()
            .MapWhenNotNullOrEmpty(s => LongSingleLineString.Create(s).Value);
        
        config.NewConfig<string, LongMultiLineString>()
            .MapWhenNotNullOrEmpty(s => LongMultiLineString.Create(s).Value);

        config.NewConfig<CreateScheduleRequest, CreateScheduleProps>()
            .Map(d => d.OnStartLearningDescription, s => s.Description)
            .IgnoreNullValues(true);

        config.NewConfig<UpdateScheduleRequest, UpdateScheduleProps>()
            .Map(d => d.OnStartLearningDescription, s => s.Description)
            .IgnoreNullValues(true);
    }
}

public class CreateScheduleRequestValidator : AbstractValidator<CreateScheduleRequest>
{
    public CreateScheduleRequestValidator()
    {
        RuleFor(p => p.CardsCountPerPhase).GreaterThanOrEqualTo((short)1);
        RuleFor(p => p.ForgottenBehavior).Must(b => Enum.IsDefined(typeof(ForgottenBehavior), b));
        RuleFor(p => p.Title).ShouldBeCreatable(ScheduleTitle.Create);
        RuleForEach(p => p.Phases).SetValidator(new CreatePhaseDtoValidator());
        
        RuleFor(p => p.ShortDescription).ShouldBeCreatable(LongSingleLineString.Create).WhenNotNullOrEmpty();
        RuleFor(p => p.DefaultPhaseShortDescription).ShouldBeCreatable(LongSingleLineString.Create).WhenNotNullOrEmpty();
        RuleFor(p => p.DefaultRepeatPhaseShortDescription).ShouldBeCreatable(LongSingleLineString.Create).WhenNotNullOrEmpty();
        
        RuleFor(p => p.Description).ShouldBeCreatable(LongMultiLineString.Create).WhenNotNullOrEmpty();
        RuleFor(p => p.DefaultPhaseDescription).ShouldBeCreatable(LongMultiLineString.Create).WhenNotNullOrEmpty();
        RuleFor(p => p.DefaultRepeatPhaseDescription).ShouldBeCreatable(LongMultiLineString.Create).WhenNotNullOrEmpty();
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
    
    public bool MoveToStartWhenPossibleFeatureFlag { get; set; }
}