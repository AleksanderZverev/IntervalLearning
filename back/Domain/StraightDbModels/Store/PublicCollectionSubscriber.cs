using System.ComponentModel.DataAnnotations.Schema;

namespace DB.Models.Store;

interface ICreatePublicCollectionSubscriberItem
{
    public long ParentUserId { get; }
    public short ParentCollectionId { get; }
    public long SubscriberUserId { get; }
}

[Table("PublicCollectionSubscriber")]
public class PublicCollectionSubscriber : IParentCollectionReference, ICreatePublicCollectionSubscriberItem
{
    public long ParentUserId { get; set; }
    public UserEntity? ParentUser { get; set; }
    public short ParentCollectionId { get; set; }
    public CollectionEntity? ParentCollection { get; set; }

    public CollectionPublicationEntity? CollectionPublication { get; set; }

    public long SubscriberUserId { get; set; }
    public UserEntity? SubscriberUser { get; set; }

    public bool IsLiked { get; set; }
    public bool IsDisliked { get; set; }
    public bool IsAdded { get; set; }
}

public class CreatePublicCollectionSubscriber : ICreatePublicCollectionSubscriberItem
{
    public long ParentUserId { get; }
    public short ParentCollectionId { get; }
    public long SubscriberUserId { get; }

    public CreatePublicCollectionSubscriber(long parentUserId, short parentCollectionId, long subscriberUserId)
    {
        ParentUserId = parentUserId;
        ParentCollectionId = parentCollectionId;
        SubscriberUserId = subscriberUserId;
    }
}

