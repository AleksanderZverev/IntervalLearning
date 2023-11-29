using Application.Common.Interfaces.Domain.Dictionary.Words;
using DB.Models.Dictionary;
using FluentResults;
using Infrastructure.Errors;

namespace Application.Commands.Dictionary.SearchWords;

public class SearchWordsCommand : ICommand<SearchWordsRequest, List<LanguageWord>>
{
    private readonly IWordsQueryResolver wordsQueryResolver;

    public SearchWordsCommand(IWordsQueryResolver wordsQueryResolver)
    {
        this.wordsQueryResolver = wordsQueryResolver;
    }

    public async Task<Result<List<LanguageWord>>> Handle(SearchWordsRequest request)
    {
        return request.Type switch
        {
            SearchWordType.Word => await wordsQueryResolver.SearchWord(request.Text, request.Count),
            SearchWordType.Pronunciation => await wordsQueryResolver.SearchWordByPronunciation(request.Text, request.Count),
            _ => new BadRequestError("Unknown search type"),
        };
    }
}