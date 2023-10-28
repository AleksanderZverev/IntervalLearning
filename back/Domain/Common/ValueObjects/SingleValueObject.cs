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

    public override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }

    public static implicit operator TValue(SingleValueObject<TValue> valueObject) 
        => valueObject.Value;
}