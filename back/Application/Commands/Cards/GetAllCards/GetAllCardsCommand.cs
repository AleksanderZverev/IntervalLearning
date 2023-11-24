using Application.Common.Interfaces.Domain.Cards;
using Domain.Card;
using FluentResults;

namespace Application.Commands.Cards.GetAllCards;

public class GetAllCardsCommand : ICommand<GetAllCardsRequest, List<Card>>
{
    private readonly ICardsQueryResolver cardsQueryResolver;

    public GetAllCardsCommand(ICardsQueryResolver cardsQueryResolver)
    {
        this.cardsQueryResolver = cardsQueryResolver;
    }
    
    public async Task<Result<List<Card>>> Handle(GetAllCardsRequest request)
    {
        var collectionCards = await cardsQueryResolver.GetAll(request.UserId, request.CollectionId);
        
        var toSkip = (request.Page - 1) * request.Count;
        return collectionCards
            .OrderByDescending(c => c.CreatedDate)
            .Skip(toSkip)
            .Take(request.Count)
            .ToList();
    }
}