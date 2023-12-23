using Domain.Card.ValueObjects;
using Domain.Collection.ValueObjects;
using FluentValidation;
using IntervalLearningApi.Extensions;

namespace IntervalLearningApi.Controllers.Study.Cards.Requests;

public class MoveCardRequestValidator : AbstractValidator<MoveCardRequest>
{
    public MoveCardRequestValidator()
    {
        RuleFor(r => r.CardId).ShouldBeCreatable(CardId.Create);
        RuleFor(r => r.DestinationCollectionId).ShouldBeCreatable(CollectionId.Create);
    }
}

public class MoveCardRequest
{
    public short DestinationCollectionId { get; set; }
    public short CardId { get; set; }
}