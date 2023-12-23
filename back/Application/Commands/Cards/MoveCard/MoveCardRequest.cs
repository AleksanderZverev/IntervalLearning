using Domain.Card.ValueObjects;
using Domain.Collection.ValueObjects;
using Domain.User.ValueObjects;

namespace Application.Commands.Cards.MoveCard;

public record MoveCardRequest(
    UserId UserId,
    CollectionId SourceCollectionId,
    CollectionId DestinationCollectionId,
    CardId CardId
);