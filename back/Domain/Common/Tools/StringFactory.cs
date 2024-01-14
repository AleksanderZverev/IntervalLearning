using FluentResults;
using GlobalTools;

namespace Domain.Common.Tools;

public class StringFactory
{
    public record class Settings
    {
        public int? MaxLength { get; init; }
        public bool RemoveExcessWhiteSpaces { get; init; }
        public bool LeaveNewLines { get; init; }
        public bool AllowEmpty { get; init; }
        public required string FieldName { get; init; }
    }

    public static Result<string> Create(string text, Settings settings)
    {
        text = text.Trim();
        
        if (settings.RemoveExcessWhiteSpaces)
        {
            text = settings.LeaveNewLines
                ? TextMaster.RemoveWhiteSpacesExceptNewLines(text)
                : TextMaster.RemoveWhiteSpaces(text);
        }
        
        if (!settings.AllowEmpty && string.IsNullOrWhiteSpace(text))
            return Result.Fail($"{settings.FieldName} is empty");

        if (settings.MaxLength != null && text.Length > settings.MaxLength)
            return Result.Fail($"{settings.FieldName} is too long");

        return text;
    }
}