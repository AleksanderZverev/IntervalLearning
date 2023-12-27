using Domain.Collection.ValueObjects;
using Domain.User.ValueObjects;

namespace Application.Commands.Cards.GetRelearningCards;

public record GetRelearningCardsCommandRequest(UserId UserId, CollectionId CollectionId, int Count);