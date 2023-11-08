using Domain.Common.ValueObjects;
using FluentResults;

namespace Domain.User.ValueObjects;

public class UserId : SingleValueObject<long>
{
    private UserId(long value) : base(value)
    {
    }

    public static UserId CreateEmpty()
    {
        return new UserId(0);
    }

    public static Result<UserId> Create(long id)
    {
        return new UserId(id);
    }
}