using System.Diagnostics;
using Domain;
using Domain.Card.ValueObjects;
using Domain.Common.ValueObjects;
using FluentResults;

namespace DB.Models.ValueObjects;

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
        
        if (id <= 0 || id >= 1000)
            return Result.Fail("Remember id should be between 0 and 1000");

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
        yield return ScheduleId.GetEqualityComponents();
        yield return CardId.GetEqualityComponents();
        yield return Id.GetEqualityComponents();
    }
}