using FluentResults;

namespace Domain.Common.ValueObjects;

public class ShortString : SingleValueObject<string>
{
    private ShortString(string value) : base(value)
    {
    }

    public static Result<ShortString> Create(string name)
    {
        var limitedStringResult = LimitedString.Create(name, 50);

        if (limitedStringResult.IsFailed)
            return limitedStringResult.ToResult();
    
        return new ShortString(limitedStringResult.Value.Value);
    }
}