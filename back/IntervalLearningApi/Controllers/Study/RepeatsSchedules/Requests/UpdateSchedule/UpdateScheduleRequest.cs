using System.ComponentModel.DataAnnotations;
using Domain.Common.ValueObjects.Text.MultiLine;
using Domain.Common.ValueObjects.Text.SingleLine;
using Domain.Schedule.ValueObjects;
using FluentValidation;
using IntervalLearningApi.Extensions;

namespace IntervalLearningApi.Controllers.Study.RepeatsSchedules.Requests.UpdateSchedule;

public class UpdateScheduleRequestValidator : AbstractValidator<UpdateScheduleRequest>
{
    public UpdateScheduleRequestValidator()
    {
        RuleFor(p => p.CardsCountPerPhase).GreaterThanOrEqualTo((short)1);
        RuleFor(p => p.Title).ShouldBeCreatable(ScheduleTitle.Create);
        RuleForEach(p => p.Phases).SetValidator(new UpdatePhaseDtoValidator());
        
        RuleFor(p => p.ShortDescription).ShouldBeCreatable(LongSingleLineString.Create).WhenNotNullOrEmpty();
        RuleFor(p => p.DefaultPhaseShortDescription).ShouldBeCreatable(LongSingleLineString.Create).WhenNotNullOrEmpty();
        RuleFor(p => p.DefaultRepeatPhaseShortDescription).ShouldBeCreatable(LongSingleLineString.Create).WhenNotNullOrEmpty();
        
        RuleFor(p => p.Description).ShouldBeCreatable(LongMultiLineString.Create).WhenNotNullOrEmpty();
        RuleFor(p => p.DefaultPhaseDescription).ShouldBeCreatable(LongMultiLineString.Create).WhenNotNullOrEmpty();
        RuleFor(p => p.DefaultRepeatPhaseDescription).ShouldBeCreatable(LongMultiLineString.Create).WhenNotNullOrEmpty();
    }
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