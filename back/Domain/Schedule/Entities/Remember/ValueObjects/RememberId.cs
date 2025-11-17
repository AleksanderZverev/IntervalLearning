using System.Diagnostics;
using Domain.Card.ValueObjects;
using Domain.Common.ValueObjects;
using Domain.Schedule.ValueObjects;
using FluentResults;

namespace Domain.Schedule.Entities.Remember.ValueObjects;

public class RememberId : SingleValueObject<short>
{
    private RememberId(short value) : base(value)
    {
    }

    public static Result<RememberId> Create(short id)
    {
        if (id == default)
        {
            Debug.Fail("Passed default id");
            return Result.Fail("Incorrect remember id");
        }

        if (id <= 0)
            return Result.Fail("Remember id can't be less or equal 0");

        return new RememberId(id);
    }
}

public class ComplexRememberId : ValueObject
{
    public required ComplexScheduleId ScheduleId { get; init; }
    public required ComplexCardId CardId { get; init; }
    public required RememberId Id { get; init; }

    public override IEnumerable<object> GetEqualityComponents()
    {
        return ScheduleId.GetEqualityComponents()
            .Concat(CardId.GetEqualityComponents())
            .Concat(Id.GetEqualityComponents());
    }
}