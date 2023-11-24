using Domain.Collection.ValueObjects;
using Domain.User.ValueObjects;

namespace Application.Commands.Cards.GetAllCards;

public record GetAllCardsRequest(
    UserId UserId,
    CollectionId CollectionId,
    int Page,
    int Count 
);