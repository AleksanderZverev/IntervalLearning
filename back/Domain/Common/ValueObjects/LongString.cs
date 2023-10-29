using FluentResults;

namespace Domain.Common.ValueObjects;

public class LongString : SingleValueObject<string>
{
    private LongString(string value) : base(value)
    {
    }

    public static Result<LongString> Create(string name)
    {
        var limitedStringResult = LimitedString.Create(name, 500);

        if (limitedStringResult.IsFailed)
            return limitedStringResult.ToResult();
    
        return new LongString(limitedStringResult.Value.Value);
    }
}