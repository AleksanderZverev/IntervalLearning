namespace IntervalLearningApi.Models.Dictionary;

public class TranslationDto
{
    public string LanguageId { get; }
    public string Id { get; }
    public string Translation { get; }

    public TranslationDto(short languageId, short id, string translation)
    {
        LanguageId = languageId.ToString();
        Id = id.ToString();
        Translation = translation;
    }
}