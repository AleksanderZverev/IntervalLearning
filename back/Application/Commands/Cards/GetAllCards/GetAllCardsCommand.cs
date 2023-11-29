using Application.Common.Interfaces.DB.Queries.Store;
using Application.Common.Interfaces.DB.Repositories.Study;
using Application.Common.Interfaces.Domain.Cards;
using Domain.Card;
using FluentResults;

namespace Application.Commands.Cards.GetAllCards;

public class GetAllCardsCommand : ICommand<GetAllCardsRequest, List<Card>>
{
    private readonly IStudyQueryRepository studyQueryRepository;

    public GetAllCardsCommand(IStudyQueryRepository studyQueryRepository)
    {
        this.studyQueryRepository = studyQueryRepository;
    }
    
    public async Task<Result<List<Card>>> Handle(GetAllCardsRequest request)
    {
        var collectionCards = await studyQueryRepository.Cards.GetAll(request.UserId, request.CollectionId);
        
        var toSkip = (request.Page - 1) * request.Count;
        return collectionCards
            .OrderByDescending(c => c.CreatedDate)
            .Skip(toSkip)
            .Take(request.Count)
            .ToList();
    }
}