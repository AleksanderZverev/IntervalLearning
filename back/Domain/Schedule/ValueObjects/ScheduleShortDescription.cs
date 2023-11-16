using Domain.Common.Tools;
using Domain.Common.ValueObjects;
using FluentResults;

namespace DB.Models.ValueObjects;

public class ScheduleShortDescription : SingleValueObject<string>
{
    private ScheduleShortDescription(string value) : base(value)
    {
    }
    
    private static StringFactory.Settings settings = new()
    {
        FieldName = "Schedule short description",
        MaxLength = 200,
        RemoveExcessWhiteSpaces = true,
        LeaveNewLines = false,
    };
    
    public static Result<ScheduleShortDescription> Create(string text)
    {
        return StringFactory.Create(text, settings)
            .Map(validatedString => new ScheduleShortDescription(validatedString));
    }
}