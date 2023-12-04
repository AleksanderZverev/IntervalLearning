using Domain.Common.Tools;
using Domain.Common.ValueObjects;
using FluentResults;

namespace Domain.Dictionary.Word.ValueObjects;

public class WordText : SingleValueObject<string>
{
    private WordText(string value) : base(value)
    {
    }

    private static StringFactory.Settings settings = new()
    {
        FieldName = "Word text",
        MaxLength = 255,
        RemoveExcessWhiteSpaces = true,
        LeaveNewLines = false,
    };

    public static Result<WordText> Create(string text)
    {
        text = text.ToLowerInvariant();
    
        return StringFactory.Create(text, settings)
            .Map(validatedString => new WordText(validatedString));
    }
}