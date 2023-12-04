namespace Domain;

public interface IDomainEvent
{
}

public interface IEntity
{
    IReadOnlyCollection<IDomainEvent> DomainEvents { get; }
    public void ClearDomainEvents();
}

public class Entity<TId> : IEquatable<Entity<TId>>, IEntity
    where TId : notnull
{
    protected Entity()
    {
        //For EF
    }

    public TId Id { get; }
    
    private List<IDomainEvent> domainEvents = new();
    public IReadOnlyCollection<IDomainEvent> DomainEvents => domainEvents.AsReadOnly();

    protected Entity(TId id)
    {
        Id = id;
    }

    public void AddDomainEvent(IDomainEvent domainEvent)
    {
        domainEvents.Add(domainEvent);
    }

    public void ClearDomainEvents()
    {
        domainEvents = new();
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

    public static bool operator ==(Entity<TId>? first, Entity<TId>? second)
    {
        return Equals(first, second);
    }

    public static bool operator !=(Entity<TId>? first, Entity<TId>? second)
    {
        return !Equals(first, second);
    }

    private static bool Equals(Entity<TId>? first, Entity<TId>? second)
    {
        if (ReferenceEquals(first, second))
            return true;
        
        if (ReferenceEquals(first, null) || ReferenceEquals(second, null))
            return false;

        return first.IsEntitiesEqual(second);
    }
}