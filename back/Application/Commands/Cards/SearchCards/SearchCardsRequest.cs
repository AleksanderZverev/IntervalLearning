using Domain.Collection.ValueObjects;
using Domain.User.ValueObjects;

namespace Application.Commands.Cards.SearchCards;

public record SearchCardsRequest(
    UserId UserId,
    CollectionId CollectionId,
    string SearchValue,
    SearchFieldType FieldType,
    int Page,
    int Count
);