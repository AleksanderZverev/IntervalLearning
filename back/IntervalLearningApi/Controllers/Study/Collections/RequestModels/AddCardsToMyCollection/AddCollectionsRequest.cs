using Domain.Collection.ValueObjects;
using FluentValidation;
using IntervalLearningApi.Extensions;

namespace IntervalLearningApi.Controllers;

public class AddCollectionsRequestValidator : AbstractValidator<AddCollectionsRequest>
{
    public AddCollectionsRequestValidator()
    {
        RuleFor(p => p.MyCollectionId).ShouldBeCreatableWhenNotNull(CollectionId.Create);
        RuleFor(p => p.NewCollectionName).NotEmpty().When(p => p.MyCollectionId == null);
    }
}

public class AddCollectionsRequest
{
    public bool CheckUnique { get; set; }
    public short? MyCollectionId { get; set; }
    public string? NewCollectionName { get; set; }
}