using Domain.Card;
using DomainServices.DB.Queries.Study;
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