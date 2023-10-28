using FluentResults;

namespace Domain.Common.ValueObjects;

public class ShortString
{
    public string Value { get; }

    protected ShortString(string value)
    {
        Value = value;
    }

    public static Result<ShortString> Create(string name)
    {
        var limitedStringResult = LimitedString.Create(name, 50);

        if (limitedStringResult.IsFailed)
            return limitedStringResult.ToResult();
    
        return new ShortString(limitedStringResult.Value.Value);
    }
}