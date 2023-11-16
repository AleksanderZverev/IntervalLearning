using FluentResults;

namespace Domain.Common.ValueObjects;

public class LimitedString : SingleValueObject<string>
{
    private LimitedString(string value) : base(value)
    {
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

