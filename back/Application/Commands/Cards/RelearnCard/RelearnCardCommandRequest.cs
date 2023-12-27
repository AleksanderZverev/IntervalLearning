using Domain.Card.ValueObjects;
using Domain.Collection.ValueObjects;
using Domain.User.ValueObjects;

namespace Application.Commands.Cards.RelearnCard;

public record RelearnCardCommandRequest(UserId UserId, CollectionId CollectionId, CardId CardId);