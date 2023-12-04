using System.ComponentModel.DataAnnotations;
using Domain.Collection.ValueObjects;
using Domain.Theme.ValueObjects;
using FluentValidation;
using IntervalLearningApi.Extensions;

namespace IntervalLearningApi.Controllers.Study.Collections.RequestModels.CreateCollection;

public class CreateCollectionRequestValidator : AbstractValidator<CreateCollectionRequest>
{
    public CreateCollectionRequestValidator()
    {
        RuleFor(p => p.CollectionId).ShouldBeCreatableWhenNotNull(CollectionId.Create);
        RuleFor(p => p.ThemeId).ShouldBeCreatable(ThemeId.Create);
        RuleFor(p => p.Title).ShouldBeCreatable(CollectionTitle.Create);
    }
}

public class CreateCollectionRequest
{
    public short? CollectionId { get; set; }
    [Required]
    public short ThemeId { get; set; }
    [Required]
    [StringLength(100)]
    public string Title { get; set; }
    public bool IsDefaultBackSide { get; set; }
}