using Domain.Common.Tools;
using FluentResults;

namespace Domain.Common.ValueObjects.Text.SingleLine;

public class MediumSingleLineString : SingleValueObject<string>
{
    private MediumSingleLineString(string value) : base(value)
    {
    }

    private static StringFactory.Settings settings = new()
    {
        FieldName = "Medium single line",
        MaxLength = 255,
        RemoveExcessWhiteSpaces = true,
        LeaveNewLines = false,
    };
    
    public static Result<MediumSingleLineString> Create(string text)
    {
        return StringFactory.Create(text, settings)
            .Map(validatedString => new MediumSingleLineString(validatedString));
    }
}