using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DB.Models.Store;

public interface ICreateCollectionPublication
{
    public long ParentUserId { get; }
    public short ParentCollectionId { get; }
}

[Table("CollectionPublications")]
public class CollectionPublicationEntity : IParentCollectionReference, ICreateCollectionPublication
{
    public long ParentUserId { get; set; }
    public UserEntity? ParentUser { get; set; }
    public short ParentCollectionId { get; set; }
    public CollectionEntity? ParentCollection { get; set; }

    [Required] 
    public DateOnly PublishDate { get; set; } = DateOnly.FromDateTime(DateTime.UtcNow);

    public uint SubscribersCount { get; set; }
    public uint LikesCount { get; set; }
    public uint DislikesCount { get; set; }

    public List<PublicCollectionSubscriber> Subscribers { get; set; } = new();
}

public class CreateCollectionPublication : ICreateCollectionPublication
{
    public long ParentUserId { get; }
    public short ParentCollectionId { get; }

    public CreateCollectionPublication(long parentUserId, short parentCollectionId)
    {
        ParentUserId = parentUserId;
        ParentCollectionId = parentCollectionId;
    }
}
