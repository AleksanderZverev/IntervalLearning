using FluentResults;

namespace Domain.User.ValueObjects;

public class UserName : ValueObject
{
    public PartedName FirstName { get; }
    public PartedName LastName { get; }

    private UserName(PartedName firstName, PartedName lastName)
    {
        FirstName = firstName;
        LastName = lastName;
    }

    public static Result<UserName> Create(string firstName, string lastName)
    {
        var partedFirstName = PartedName.Create(firstName);

        if (partedFirstName.IsFailed)
            return partedFirstName.ToResult();

        var partedLastName = PartedName.Create(lastName);

        if (partedLastName.IsFailed)
            return partedLastName.ToResult();

        return new UserName(partedFirstName.Value, partedLastName.Value);
    }

    public override IEnumerable<object> GetEqualityComponents()
    {
        yield return FirstName;
        yield return LastName;
    }
}