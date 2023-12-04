namespace Domain.Common.ValueObjects;

public abstract class SingleValueObject<TValue> : ValueObject, IComparable
    where TValue : notnull, IComparable
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

    public int CompareTo(object? obj)
    {
        if (ReferenceEquals(obj, null))
            return 1;

        if (obj is not SingleValueObject<TValue> singleValueObject)
            return 1;
        
        return Value.CompareTo(singleValueObject.Value);
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