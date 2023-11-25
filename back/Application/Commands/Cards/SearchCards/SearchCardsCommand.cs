using Application.Common.Interfaces.Domain.Cards;
using Domain.Card;
using Domain.Collection.ValueObjects;
using Domain.User.ValueObjects;
using FluentResults;

namespace Application.Commands.Cards.SearchCards;

public enum SearchFieldType
{
    RememberingText,
    PromptText,
    MeaningText
}

public record SearchCardsRequest(
    UserId userId,
    CollectionId collectionId,
    string searchValue,
    SearchFieldType fieldType,
    int page,
    int count
);

public class SearchCardsCommand : ICommand<SearchCardsRequest, List<Card>>
{
    private readonly ICardsQueryResolver cardsQueryResolver;

    public SearchCardsCommand(
        ICardsQueryResolver cardsQueryResolver)
    {
        this.cardsQueryResolver = cardsQueryResolver;
    }

    public async Task<Result<List<Card>>> Handle(SearchCardsRequest request)
    {
        return await cardsQueryResolver.Search(
            request.userId,
            request.collectionId,
            request.searchValue,
            request.fieldType,
            request.page,
            request.count);
    }
}