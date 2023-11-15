using FluentResults;

namespace Domain.Common.ValueObjects;

public class LimitedString
{
    public string Value { get; }

    protected LimitedString(string value)
    {
        Value = value;
    }

    public static Result<LimitedString> Create(string text, int length)
    {
        if (string.IsNullOrWhiteSpace(text))
            return Result.Fail("String is empty");
        
        text = text.Trim();

        if (text.Length > length)
            return Result.Fail("String is too long");

        return new LimitedString(text);
    }
}

