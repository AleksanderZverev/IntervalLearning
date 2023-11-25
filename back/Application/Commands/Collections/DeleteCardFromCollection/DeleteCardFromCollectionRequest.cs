using Domain.Card.ValueObjects;
using Domain.Collection.ValueObjects;
using Domain.User.ValueObjects;

namespace Application.Commands.Collections.DeleteCardFromCollection;

public record DeleteCardFromCollectionRequest(
    UserId UserId,
    CollectionId CollectionId,
    CardId CardId
);