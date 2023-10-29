using FluentResults;

namespace Domain.Common.ValueObjects;

public class MediumString : SingleValueObject<string>
{
    private MediumString(string value) : base(value)
    {
    }

    public static Result<MediumString> Create(string name)
    {
        var limitedStringResult = LimitedString.Create(name, 255);

        if (limitedStringResult.IsFailed)
            return limitedStringResult.ToResult();
    
        return new MediumString(limitedStringResult.Value.Value);
    }
}