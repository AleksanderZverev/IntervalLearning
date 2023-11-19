using Domain.Common.Tools;
using Domain.Common.ValueObjects;
using FluentResults;

namespace DB.Models.Dictionary.ValueObjects;

public class WordPronunciation : SingleValueObject<string>
{
    private WordPronunciation(string value) : base(value)
    {
    }

    private static StringFactory.Settings settings = new()
    {
        FieldName = "Word pronunciation",
        MaxLength = 255,
        RemoveExcessWhiteSpaces = true,
        LeaveNewLines = false,
    };

    public static Result<WordPronunciation> Create(string text)
    {
        text = text.ToLowerInvariant();
    
        return StringFactory.Create(text, settings)
            .Map(validatedString => new WordPronunciation(validatedString));
    }
}