using Domain.Schedule.ValueObjects;
using Domain.User.ValueObjects;
using FluentValidation;
using IntervalLearningApi.Extensions;

namespace IntervalLearningApi.Controllers.Study.Cards.Requests.RememberCard;

public class RememberCardRequestValidator : AbstractValidator<RememberCardRequest>
{
    public RememberCardRequestValidator()
    {
        RuleFor(p => p.ScheduleUserId).ShouldBeCreatable(UserId.Create);
        RuleFor(p => p.ScheduleId).ShouldBeCreatable(ScheduleId.Create);
        RuleFor(p => p.PhaseIndex).GreaterThanOrEqualTo((short)0);
        
        RuleFor(p => p.RememberItems).NotNull().NotEmpty();
        RuleForEach(p => p.RememberItems).SetValidator(new RememberItemValidator());
    }
}

public class RememberCardRequest
{
    public List<RememberItemDto> RememberItems { get; set; }
    public long ScheduleUserId { get; set; }
    public short ScheduleId { get; set; }
    public short PhaseIndex { get; set; }
}