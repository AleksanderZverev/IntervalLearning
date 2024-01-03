using Domain.Collection.ValueObjects;
using Domain.User.ValueObjects;
using DomainServices.DB.Queries.Study.Cards;

namespace Application.Commands.Cards.SearchCards;

public record SearchCardsRequest(
    UserId UserId,
    CollectionId CollectionId,
    string SearchValue,
    SearchFieldType FieldType,
    int Page,
    int Count
);