namespace Domain;

public abstract class ValueObject : IEquatable<ValueObject>
{
    public abstract IEnumerable<object> GetEqualityComponents();

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

    public override int GetHashCode()
    {
        return GetEqualityComponents()
            .Select(c => c.GetHashCode())
            .Aggregate((f, s) => f ^ s);
    }

    public static bool operator ==(ValueObject first, ValueObject second)
    {
        return first.Equals(second);
    }

    public static bool operator !=(ValueObject first, ValueObject second)
    {
        return !first.Equals(second);
    }
}