using System.Diagnostics;
using Domain.Common.ValueObjects;
using FluentResults;

namespace DB.Models.ValueObjects;

public class RememberWeight : SingleValueObject<float>
{
    private RememberWeight(float value) : base(value)
    {
    }

    public static Result<RememberWeight> Create(float weight)
    {
        if (weight < 0f)
        {
            Debug.Fail("Weight is less than 0");
            return Result.Fail("Weight cannot be less than 0");
        }

        if (weight > 1f)
        {
            Debug.Fail("Weight is more than 1");
            return Result.Fail("Weight cannot be more than 1");
        }

        return new RememberWeight(weight);
    }
}