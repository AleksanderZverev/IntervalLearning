using DB.Models.ValueObjects;
using Domain.Card.ValueObjects;
using Domain.User.ValueObjects;
using FluentValidation;
using IntervalLearningApi.Extensions;

namespace IntervalLearningApi.Controllers.Study.Cards.Requests.StartCards;

public class StartCardsRequestValidator : AbstractValidator<StartCardsRequest>
{
    public StartCardsRequestValidator()
    {
        RuleFor(p => p.ScheduleUserId).ShouldBeCreatable(UserId.Create);
        RuleFor(p => p.ScheduleId).ShouldBeCreatable(ScheduleId.Create);
        RuleFor(p => p.CardIds).ForEach(cardId => cardId.ShouldBeCreatable(CardId.Create)).WhenNotNull();
    }
}

public class StartCardsRequest
{
    public long ScheduleUserId { get; set; }
    public short ScheduleId { get; set; }
    public List<short> CardIds { get; set; }
}