using Domain.Card.ValueObjects;
using Domain.Collection.ValueObjects;
using Domain.User.ValueObjects;

namespace Application.Commands.Cards.CreateCard;

public record CreateCardRequest
{
    public required CardText RememberingText { get; init; }
    public CardText? PromptText { get; init; }
    public required CardText MeaningText { get; init; }
    public CardDescription? Description { get; init; }
    public required List<CardExample> Examples { get; init; }
    public required List<CardTag> Tags { get; init; }
    public required UserId ParentUserId { get; init; }
    public required CollectionId ParentCollectionId { get; init; }
}