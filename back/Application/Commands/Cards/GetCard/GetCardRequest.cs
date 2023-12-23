using Domain.Card.ValueObjects;
using Domain.Collection.ValueObjects;
using Domain.User.ValueObjects;

namespace Application.Commands.Cards.GetCard;

public record GetCardRequest(
    UserId UserId,
    CollectionId CollectionId,
    CardId CardId);