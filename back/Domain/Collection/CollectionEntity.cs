using Domain.Collection.ValueObjects;
using Domain.User;

namespace Domain.Collection;

public interface IParentCollectionReference : IParentUserReference
{
    public CollectionId ParentCollectionId { get; set; }
    public Collection? ParentCollection { get; set; }
}
