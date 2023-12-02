using IntervalLearningApi.Models.Dictionary;

namespace IntervalLearningApi.Controllers;

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