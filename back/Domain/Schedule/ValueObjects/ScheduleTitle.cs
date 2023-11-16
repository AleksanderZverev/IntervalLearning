using Domain.Common.Tools;
using Domain.Common.ValueObjects;
using FluentResults;

namespace DB.Models.ValueObjects;

public class ScheduleTitle : SingleValueObject<string>
{
    private ScheduleTitle(string value) : base(value)
    {
    }
    
    private static StringFactory.Settings settings = new()
    {
        FieldName = "Schedule title",
        MaxLength = 255,
        RemoveExcessWhiteSpaces = true,
    };
    
    public static Result<ScheduleTitle> Create(string text)
    {
        return StringFactory.Create(text, settings)
            .Map(validatedString => new ScheduleTitle(validatedString));
    }
}