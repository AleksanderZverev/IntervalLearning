using DB.Models;
using Domain.Collection.ValueObjects;

namespace Domain.Collection;

public interface IParentCollectionReference : IParentUserReference
{
    public CollectionId ParentCollectionId { get; set; }
    public Collection? ParentCollection { get; set; }
}
