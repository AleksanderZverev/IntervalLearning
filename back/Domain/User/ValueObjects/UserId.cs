using System.Diagnostics;
using Domain.Common.ValueObjects;
using FluentResults;

namespace Domain.User.ValueObjects;

public class UserId : SingleValueObject<long>
{
    private UserId(long value) : base(value)
    {
    }

    public static Result<UserId> Create(long id)
    {
        if (id == default)
        {
            Debug.Fail("Default value passed");
            return Result.Fail("User Id is not specified");
        }
        
        return new UserId(id);
    }
}