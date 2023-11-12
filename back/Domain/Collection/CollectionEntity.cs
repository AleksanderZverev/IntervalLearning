using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using DB.Models.Store;
using Domain.Collection;
using Domain.Collection.ValueObjects;
using Domain.User;
using Domain.User.ValueObjects;

namespace DB.Models;

public interface IParentCollectionReference : IParentUserReference
{
    public CollectionId ParentCollectionId { get; set; }
    public Collection? ParentCollection { get; set; }
}
