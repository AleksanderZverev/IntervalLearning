namespace Domain;

public abstract class ValueObject : IEquatable<ValueObject>
{
    public abstract IEnumerable<object> GetEqualityComponents();

    public override int GetHashCode()
    {
        return GetEqualityComponents()
            .Select(c => c.GetHashCode())
            .Aggregate((f, s) => f ^ s);
    }

    bool IEquatable<ValueObject>.Equals(ValueObject? other)
    {
        if (ReferenceEquals(other, null))
            return false;

        return IsComponentsEquals(other);
    }

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(obj, null) || obj.GetType() != this.GetType())
            return false;

        var valueObject = (ValueObject)obj;
        return IsComponentsEquals(valueObject);
    }

    private bool IsComponentsEquals(ValueObject other)
    {
        return GetEqualityComponents().SequenceEqual(other.GetEqualityComponents());
    }

    public static bool operator ==(ValueObject? first, ValueObject? second)
    {
        return Equals(first, second);
    }

    public static bool operator !=(ValueObject? first, ValueObject? second)
    {
        return !Equals(first, second);
    }
    
    private static bool Equals(ValueObject? first, ValueObject? second)
    {
        if (ReferenceEquals(first, second))
            return true;
        
        if (ReferenceEquals(first, null) || ReferenceEquals(second, null))
            return false;

        return first.Equals(second);
    }
}