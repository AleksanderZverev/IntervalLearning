using Domain.Card.ValueObjects;
using Domain.Collection.ValueObjects;
using Domain.User.ValueObjects;

namespace Application.Commands.Collections.MoveCollectionCard;

public record MoveCollectionCardRequest(
    UserId UserId,
    CollectionId SourceCollectionId,
    CollectionId DestinationCollectionId,
    CardId CardId
);