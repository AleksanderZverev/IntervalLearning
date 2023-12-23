using System.Diagnostics;
using Domain.Common.ValueObjects;
using Domain.User.ValueObjects;
using FluentResults;

namespace Domain.Collection.ValueObjects;

public class CollectionId : SingleValueObject<short>
{
    private CollectionId(short value) : base(value)
    {
    }
    
    public static Result<CollectionId> Create(short id)
    {
        if (id == default)
        {
            Debug.Fail("Default value passed");
            return Result.Fail("Collection Id is not specified");
        }
        
        return new CollectionId(id);
    }
}

public class ComplexCollectionId : ValueObject
{
    private ComplexCollectionId(UserId userId, CollectionId id)
    {
        UserId = userId;
        Id = id;
    }

    public UserId UserId { get; }
    public CollectionId Id { get; }
    
    
    public static Result<ComplexCollectionId> Create(UserId userId, CollectionId id)
    {
        return new ComplexCollectionId(userId, id);
    }

    public override IEnumerable<object> GetEqualityComponents()
    {
        yield return UserId.GetEqualityComponents();
        yield return Id.GetEqualityComponents();
    }
}