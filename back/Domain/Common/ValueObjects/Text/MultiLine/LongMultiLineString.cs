using Domain.Common.Tools;
using FluentResults;

namespace Domain.Common.ValueObjects.Text.MultiLine;

public class LongMultiLineString : SingleValueObject<string>
{
    private LongMultiLineString(string value) : base(value)
    {
    }
    
    private static StringFactory.Settings settings = new()
    {
        FieldName = "Long multi line string",
        RemoveExcessWhiteSpaces = true,
        LeaveNewLines = true,
    };
    
    public static Result<LongMultiLineString> Create(string text)
    {
        return StringFactory.Create(text, settings)
            .Map(validatedString => new LongMultiLineString(validatedString));
    }
}