using Domain.Card.ValueObjects;
using Domain.Collection.ValueObjects;
using Domain.User.ValueObjects;

namespace Application.Commands.Cards.UpdateCard;

public record UpdateCardRequest
{
    public CardId CardId { get; init; }
    public CardText RememberingText { get; init; }
    public CardText? PromptText { get; init; }
    public CardText MeaningText { get; init; }
    public CardDescription? Description { get; init; }
    public List<CardExample> Examples { get; init; }
    public UserId ParentUserId { get; init; }
    public CollectionId ParentCollectionId { get; init; }
}