using Domain.Dictionary.Translation.ValueObjects;
using Domain.Dictionary.Word;
using Domain.Language.ValueObjects;

namespace Domain.Dictionary.Translation;

public class WordTranslation
{
    public int WordId { get; set; }
    public LanguageWord? Word { get; set; }

    public LanguageId LanguageId { get; set; }
    public Language.Language? Language { get; set; }

    public short Id { get; set; }
    public required TranslationText Translation { get; set; }
}