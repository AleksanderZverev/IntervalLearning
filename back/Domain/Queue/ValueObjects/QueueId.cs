using System.Diagnostics;
using Domain.Card.ValueObjects;
using Domain.Common.ValueObjects;
using Domain.Schedule.ValueObjects;
using FluentResults;

namespace Domain.Queue.ValueObjects;

public class QueueId : SingleValueObject<short>
{
    private QueueId(short value) : base(value)
    {
    }

    public static Result<QueueId> Create(short id)
    {
        if (id == default)
        {
            Debug.Fail("Passed empty id");
            return Result.Fail("Incorrect queue id");
        }
        
        return new QueueId(id);
    }
}

public class ComplexQueueId : ValueObject
{
    public required ComplexScheduleId ScheduleId { get; init; }
    public required ComplexCardId CardId { get; init; }
    public required QueueId QueueId { get; init; }
    
    public override IEnumerable<object> GetEqualityComponents()
    {
        yield return ScheduleId.GetEqualityComponents();
        yield return CardId.GetEqualityComponents();
        yield return QueueId.GetEqualityComponents();
    }
}