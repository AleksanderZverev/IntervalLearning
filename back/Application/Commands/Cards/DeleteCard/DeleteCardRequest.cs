using Domain.Card.ValueObjects;
using Domain.Collection.ValueObjects;
using Domain.User.ValueObjects;

namespace Application.Commands.Cards.DeleteCard;

public record DeleteCardRequest(
    UserId UserId,
    CollectionId CollectionId,
    CardId CardId
);