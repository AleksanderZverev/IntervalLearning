using Domain.Card.ValueObjects;
using FluentValidation;
using IntervalLearningApi.Extensions;

namespace IntervalLearningApi.Controllers.Study.Cards.Requests.RememberCard;

public class RememberItemValidator : AbstractValidator<RememberItemDto>
{
    public RememberItemValidator()
    {
        RuleFor(p => p.CardId).ShouldBeCreatable(CardId.Create);
        RuleFor(p => p.Weight).InclusiveBetween(0.0f, 1.0f);
        RuleFor(p => p.Comment).MaximumLength(255).WhenNotNullOrEmpty();
    }
}

public class RememberItemDto
{
    public short CardId { get; set; }
    public float Weight { get; set; }
    public string? Comment { get; set; }
}