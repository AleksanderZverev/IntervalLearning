using IntervalLearningApi.Controllers.Dictionary.DTOs;

namespace IntervalLearningApi.Controllers.Study.Collections.RequestModels.GetRandomWords;

public class GetRandomWordResponse
{
    public List<WordDto> Words { get; }

    public LanguageDto Language { get; }

    public GetRandomWordResponse(
        List<WordDto> words, 
        LanguageDto language)
    {
        Words = words;
        Language = language;
    }
}