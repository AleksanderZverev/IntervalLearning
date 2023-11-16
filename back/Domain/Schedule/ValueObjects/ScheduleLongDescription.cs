using Domain.Common.Tools;
using Domain.Common.ValueObjects;
using FluentResults;

namespace DB.Models.ValueObjects;

public class ScheduleLongDescription : SingleValueObject<string>
{
    private ScheduleLongDescription(string value) : base(value)
    {
    }
    
    private static StringFactory.Settings settings = new()
    {
        FieldName = "Schedule long description",
        RemoveExcessWhiteSpaces = true,
        LeaveNewLines = true,
    };
    
    public static Result<ScheduleLongDescription> Create(string text)
    {
        return StringFactory.Create(text, settings)
            .Map(validatedString => new ScheduleLongDescription(validatedString));
    }
}