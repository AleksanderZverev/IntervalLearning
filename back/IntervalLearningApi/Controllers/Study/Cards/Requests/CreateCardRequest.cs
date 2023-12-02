using System.ComponentModel.DataAnnotations;
using Domain.Card.ValueObjects;
using FluentValidation;
using IntervalLearningApi.Extensions;

namespace IntervalLearningApi.Controllers;

public class CreateCardRequestValidator : AbstractValidator<CreateCardRequest>
{
    public CreateCardRequestValidator()
    {
        RuleFor(p => p.CardId).ShouldBeCreatableWhenNotNull(CardId.Create);
        RuleFor(p => p.FrontText).ShouldBeCreatable(CardText.Create);
        RuleFor(p => p.PromptText).ShouldBeCreatable(CardText.Create).WhenNotNull();
        RuleFor(p => p.BackText).ShouldBeCreatable(CardText.Create);
        RuleFor(p => p.Description).ShouldBeCreatable(CardDescription.Create).WhenNotNull();
        RuleFor(p => p.Examples).ForEach(e => e.ShouldBeCreatable(CardExample.Create)).WhenNotNull();
    }
}

public class CreateCardRequest
{
    public short? CardId { get; set; }
    [Required]
    [StringLength(255)]
    public string FrontText { get; set; }

    [StringLength(255)] 
    public string? PromptText { get; set; }

    [Required]
    [StringLength(255)]
    public string BackText { get; set; }
    [StringLength(500)]
    public string? Description { get; set; }

    [MaxLength(15)]
    public List<string>? Examples { get; set; }
}