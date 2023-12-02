using FluentValidation;

namespace IntervalLearningApi.Models.RepeatsSchedule;

public class UpdatePhaseDtoValidator : BasePhaseBodyValidator<UpdatePhaseDto>
{
}

public class UpdatePhaseDto : BasePhaseBody
{
}