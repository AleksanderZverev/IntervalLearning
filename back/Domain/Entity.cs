using Domain.User.Events;

namespace Domain;

public interface IDomainEvent
{
    
}

public class Entity<TId> : IEquatable<Entity<TId>>
    where TId : notnull
{
    public TId Id { get; }
    
    private readonly List<IDomainEvent> domainEvents = new();
    public IReadOnlyCollection<IDomainEvent> DomainEvents => domainEvents.AsReadOnly();

    protected Entity(TId id)
    {
        Id = id;
    }

    public void AddDomainEvent(IDomainEvent domainEvent)
    {
        domainEvents.Add(domainEvent);
    }

    public override int GetHashCode()
    {
        return Id.GetHashCode();
    }

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(obj, null))
            return false;

        if (obj is Entity<TId> entity)
            return IsEntitiesEqual(entity);

        return false;
    }

    public bool Equals(Entity<TId>? other)
    {
        if (ReferenceEquals(other, null))
            return false;

        return IsEntitiesEqual(other);
    }

    private bool IsEntitiesEqual(Entity<TId> other)
    {
        return Id.Equals(other.Id);
    }

    public static bool operator ==(Entity<TId> first, Entity<TId> second)
    {
        return first.IsEntitiesEqual(second);
    }

    public static bool operator !=(Entity<TId> first, Entity<TId> second)
    {
        return !first.IsEntitiesEqual(second);
    }
}