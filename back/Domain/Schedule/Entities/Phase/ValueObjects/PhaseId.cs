using System.Diagnostics;
using Domain;
using Domain.Common.ValueObjects;
using Domain.User.ValueObjects;
using FluentResults;

namespace DB.Models.ValueObjects;

public class PhaseId : SingleValueObject<short>
{
    private PhaseId(short value) : base(value)
    {
        
    }

    public static Result<PhaseId> Create(short id)
    {
        if (id == default)
        {
            Debug.Fail("Passed default id");
            return Result.Fail("Incorrect phase id");
        }
        
        if (id <= 0 || id >= 1000)
            return Result.Fail("Phase should be between 0 and 1000");

        return new PhaseId(id);
    }
}

public class ComplexPhaseId : ValueObject
{
    public ScheduleId ParentRepeatsScheduleId { get; set; }
    public UserId ParentUserId { get; set; }
    public PhaseId Id { get; set; }
    
    public override IEnumerable<object> GetEqualityComponents()
    {
        yield return ParentRepeatsScheduleId.GetEqualityComponents()
            .Append(ParentUserId.GetEqualityComponents())
            .Append(Id.GetEqualityComponents());
    }
}