using Domain.Common.ValueObjects;
using FluentResults;

namespace Domain.User.ValueObjects;

public class PartedName : SingleValueObject<string>
{
    private PartedName(string value) : base(value)
    {
    }

    public static Result<PartedName> Create(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return Result.Fail("Name is empty");

        name = name.Trim();

        if (name.Length > 50)
            return Result.Fail("Name is too long");

        return new PartedName(name);
    }
}