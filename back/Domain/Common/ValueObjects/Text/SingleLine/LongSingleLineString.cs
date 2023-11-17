using Domain.Common.Tools;
using Domain.Common.ValueObjects;
using FluentResults;

namespace DB.Models.ValueObjects;

public class LongSingleLineString : SingleValueObject<string>
{
    private LongSingleLineString(string value) : base(value)
    {
    }
    
    private static StringFactory.Settings settings = new()
    {
        FieldName = "Single line",
        MaxLength = 255,
        RemoveExcessWhiteSpaces = true,
        LeaveNewLines = false,
    };
    
    public static Result<LongSingleLineString> Create(string text)
    {
        return StringFactory.Create(text, settings)
            .Map(validatedString => new LongSingleLineString(validatedString));
    }
}