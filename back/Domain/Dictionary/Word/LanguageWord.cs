using Domain.Dictionary.Word.ValueObjects;
using Domain.Language.ValueObjects;

namespace Domain.Dictionary.Word;

public class LanguageWord
{
    public int Id { get; init; }
    public WordText Word { get; set; }
    public WordPronunciation? Pronunciation { get; set; }


    public LanguageId LanguageId { get; set; }
    public Language.Language? Language { get; set; }
}