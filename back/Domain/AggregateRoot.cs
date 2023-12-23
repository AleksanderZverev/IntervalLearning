namespace Domain;

public class AggregateRoot<TId> : Entity<TId>
    where TId : notnull
{
    protected AggregateRoot() : base()
    {
        //For EF
    }
    
    protected AggregateRoot(TId id) : base(id)
    {
    }
}