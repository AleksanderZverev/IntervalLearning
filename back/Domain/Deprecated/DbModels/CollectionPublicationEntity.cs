using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Domain.Collection;
using Domain.Collection.ValueObjects;
using Domain.User.ValueObjects;

namespace Domain.Deprecated.DbModels;

public interface ICreateCollectionPublication
{
    public UserId ParentUserId { get; }
    public CollectionId ParentCollectionId { get; }
}

[Table("CollectionPublications")]
public class CollectionPublicationEntity : IParentCollectionReference, ICreateCollectionPublication
{
    public UserId ParentUserId { get; set; }
    public User.User? ParentUser { get; set; }
    public CollectionId ParentCollectionId { get; set; }
    public Collection.Collection? ParentCollection { get; set; }

    [Required] 
    public DateOnly PublishDate { get; set; } = DateOnly.FromDateTime(DateTime.UtcNow);

    public uint SubscribersCount { get; set; }
    public uint LikesCount { get; set; }
    public uint DislikesCount { get; set; }

    public List<PublicCollectionSubscriber> Subscribers { get; set; } = new();
}

public class CreateCollectionPublication : ICreateCollectionPublication
{
    public UserId ParentUserId { get; }
    public CollectionId ParentCollectionId { get; }

    public CreateCollectionPublication(
        UserId parentUserId,
        CollectionId parentCollectionId)
    {
        ParentUserId = parentUserId;
        ParentCollectionId = parentCollectionId;
    }
}
