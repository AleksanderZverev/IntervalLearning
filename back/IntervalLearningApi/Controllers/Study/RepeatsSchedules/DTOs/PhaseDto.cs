using System.ComponentModel.DataAnnotations;
using Application.Commands.Schedules.CreateSchedule;
using Application.Commands.Schedules.UpdateSchedule;
using DB.Models;
using DB.Models.ValueObjects;
using FluentValidation;
using IntervalLearningApi.Controllers.Study.RepeatsSchedules.Requests.CreateSchedule;
using IntervalLearningApi.Controllers.Study.RepeatsSchedules.Requests.UpdateSchedule;
using IntervalLearningApi.Extensions;
using Mapster;
using Newtonsoft.Json;

namespace IntervalLearningApi.Controllers.Study.RepeatsSchedules.DTOs;

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

public class BasePhaseBodyValidator<T> : AbstractValidator<T>
    where T : BasePhaseBody
{
    public BasePhaseBodyValidator()
    {
        RuleFor(p => p.Id).NotNull().NotEmpty();
        RuleFor(p => p.SecondsFromLastPhase).GreaterThanOrEqualTo((uint)1);
        RuleFor(p => p.ShortDescription).Length(1, 200).WhenNotNull();
        RuleFor(p => p.Description).Length(1, 200).WhenNotNull();
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

public class PhaseDto : BasePhaseBody
{
    [JsonProperty("userId")]
    public string ParentUserId { get; set; }

    [JsonProperty("scheduleId")]
    public string ParentRepeatsScheduleId { get; set; }
}