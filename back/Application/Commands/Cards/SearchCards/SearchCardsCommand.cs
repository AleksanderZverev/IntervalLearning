using Application.Common.Interfaces.Domain.Cards;
using Domain.Card;
using FluentResults;

namespace Application.Commands.Cards.SearchCards;

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
            request.UserId,
            request.CollectionId,
            request.SearchValue,
            request.FieldType,
            request.Page,
            request.Count);
    }
}