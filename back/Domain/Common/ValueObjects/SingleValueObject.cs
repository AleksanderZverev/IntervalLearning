using System.Runtime.CompilerServices;
using FluentResults;

namespace Domain.Common.ValueObjects;

public abstract class SingleValueObject<TValue> : ValueObject
    where TValue : notnull
{
    protected SingleValueObject(TValue value)
    {
        Value = value;
    }

    public TValue Value { get; }

    public override string ToString()
    {
        return Value.ToString();
    }

    public override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }

    public static implicit operator TValue?(SingleValueObject<TValue>? valueObject) 
        => ReferenceEquals(valueObject, null) 
            ? default 
            : valueObject.Value;
}