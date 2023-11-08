using System.ComponentModel.DataAnnotations.Schema;
using Domain.User;
using Domain.User.ValueObjects;

namespace DB.Models.Store;

interface ICreatePublicCollectionSubscriberItem
{
    public UserId ParentUserId { get; }
    public short ParentCollectionId { get; }
    public UserId SubscriberUserId { get; }
}

[Table("PublicCollectionSubscriber")]
public class PublicCollectionSubscriber : IParentCollectionReference, ICreatePublicCollectionSubscriberItem
{
    public UserId ParentUserId { get; set; }
    public User? ParentUser { get; set; }
    public short ParentCollectionId { get; set; }
    public CollectionEntity? ParentCollection { get; set; }

    public CollectionPublicationEntity? CollectionPublication { get; set; }

    public UserId SubscriberUserId { get; set; }
    public User? SubscriberUser { get; set; }

    public bool IsLiked { get; set; }
    public bool IsDisliked { get; set; }
    public bool IsAdded { get; set; }
}

public class CreatePublicCollectionSubscriber : ICreatePublicCollectionSubscriberItem
{
    public UserId ParentUserId { get; }
    public short ParentCollectionId { get; }
    public UserId SubscriberUserId { get; }

    public CreatePublicCollectionSubscriber(UserId parentUserId, short parentCollectionId, UserId subscriberUserId)
    {
        ParentUserId = parentUserId;
        ParentCollectionId = parentCollectionId;
        SubscriberUserId = subscriberUserId;
    }
}

