using DB.Models.Dictionary.ValueObjects;
using Domain.Language;
using Domain.Language.ValueObjects;

namespace DB.Models.Dictionary;

public class WordTranslation
{
    public int WordId { get; set; }
    public LanguageWord? Word { get; set; }

    public LanguageId LanguageId { get; set; }
    public Language? Language { get; set; }

    public short Id { get; set; }
    public required TranslationText Translation { get; set; }
}