namespace IntervalLearningApi.Models.Dictionary;

public class LanguageDto
{
    public string Id { get; }
    public string Name { get; }
    public string NativeLanguageName { get; }
    public string? TranslationLinkTitle { get; }
    public string? TranslationLink { get; }

    public LanguageDto(
        short id,
        string name,
        string nativeLanguageName,
        string? translationLinkTitle,
        string? translationLink)
    {
        Id = id.ToString();
        Name = name;
        NativeLanguageName = nativeLanguageName;
        TranslationLinkTitle = translationLinkTitle;
        TranslationLink = translationLink;
    }
}