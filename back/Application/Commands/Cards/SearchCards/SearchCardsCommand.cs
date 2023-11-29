using Application.Common.Interfaces.DB.Repositories.Study;
using Application.Common.Interfaces.Domain.Cards;
using Domain.Card;
using FluentResults;

namespace Application.Commands.Cards.SearchCards;

public class SearchCardsCommand : ICommand<SearchCardsRequest, List<Card>>
{
    private readonly IStudyQueryRepository studyQueryRepository;

    public SearchCardsCommand(
        IStudyQueryRepository studyQueryRepository)
    {
        this.studyQueryRepository = studyQueryRepository;
    }

    public async Task<Result<List<Card>>> Handle(SearchCardsRequest request)
    {
        return await studyQueryRepository.Cards.Search(
            request.UserId,
            request.CollectionId,
            request.SearchValue,
            request.FieldType,
            request.Page,
            request.Count);
    }
}