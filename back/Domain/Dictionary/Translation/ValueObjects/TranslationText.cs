using Domain.Common.Tools;
using Domain.Common.ValueObjects;
using FluentResults;

namespace Domain.Dictionary.Translation.ValueObjects;

public class TranslationText : SingleValueObject<string>
{
    private TranslationText(string value) : base(value)
    {
    }

    private static StringFactory.Settings settings = new()
    {
        FieldName = "Word translation",
        MaxLength = 255,
        RemoveExcessWhiteSpaces = true,
        LeaveNewLines = false,
    };
    
    public static Result<TranslationText> Create(string text)
    {
        text = text.ToLowerInvariant();
        
        return StringFactory.Create(text, settings)
            .Map(validatedString => new TranslationText(validatedString));
    }
}