using Application.Common.Interfaces.DB.Queries.Dictionary;
using Application.Common.Interfaces.Domain.Dictionary.Words;
using DB.Models.Dictionary;
using FluentResults;
using Infrastructure.Errors;

namespace Application.Commands.Dictionary.SearchWords;

public class SearchWordsCommand : ICommand<SearchWordsRequest, List<LanguageWord>>
{
    private readonly IDictionaryQueryRepository dictionaryQueryRepository;

    public SearchWordsCommand(IDictionaryQueryRepository dictionaryQueryRepository)
    {
        this.dictionaryQueryRepository = dictionaryQueryRepository;
    }

    public async Task<Result<List<LanguageWord>>> Handle(SearchWordsRequest request)
    {
        return request.Type switch
        {
            SearchWordType.Word => await dictionaryQueryRepository.Words.SearchWord(request.Text, request.Count),
            SearchWordType.Pronunciation => await dictionaryQueryRepository.Words.SearchWordByPronunciation(request.Text, request.Count),
            _ => new BadRequestError("Unknown search type"),
        };
    }
}