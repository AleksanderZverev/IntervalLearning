using System.Diagnostics;
using Domain.Common.ValueObjects;
using Domain.User.ValueObjects;
using FluentResults;

namespace Domain.Schedule.ValueObjects;

public class ScheduleId : SingleValueObject<short>
{
    private ScheduleId(short value) : base(value)
    {
    }

    public static Result<ScheduleId> Create(short id)
    {
        if (id == default)
        {
            Debug.Fail("Default id passed");
            return Result.Fail("Incorrect schedule id");
        }

        return new ScheduleId(id);
    }
}

public class ComplexScheduleId : ValueObject
{
    public required UserId ParentUserId { get; init; }
    public required ScheduleId Id { get; init; }

    public override IEnumerable<object> GetEqualityComponents()
    {
        return ParentUserId.GetEqualityComponents()
            .Concat(Id.GetEqualityComponents());
    }
}