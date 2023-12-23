using System.ComponentModel.DataAnnotations.Schema;
using Domain.Collection;
using Domain.Collection.ValueObjects;
using Domain.User.ValueObjects;

namespace Domain.Deprecated.DbModels;

interface ICreatePublicCollectionSubscriberItem
{
    public UserId ParentUserId { get; }
    public CollectionId ParentCollectionId { get; }
    public UserId SubscriberUserId { get; }
}

[Table("PublicCollectionSubscriber")]
public class PublicCollectionSubscriber : IParentCollectionReference, ICreatePublicCollectionSubscriberItem
{
    public UserId ParentUserId { get; set; }
    public User.User? ParentUser { get; set; }
    public CollectionId ParentCollectionId { get; set; }
    public Collection.Collection? ParentCollection { get; set; }

    public CollectionPublicationEntity? CollectionPublication { get; set; }

    public UserId SubscriberUserId { get; set; }
    public User.User? SubscriberUser { get; set; }

    public bool IsLiked { get; set; }
    public bool IsDisliked { get; set; }
    public bool IsAdded { get; set; }
}

public class CreatePublicCollectionSubscriber : ICreatePublicCollectionSubscriberItem
{
    public UserId ParentUserId { get; }
    public CollectionId ParentCollectionId { get; }
    public UserId SubscriberUserId { get; }

    public CreatePublicCollectionSubscriber(UserId parentUserId, CollectionId parentCollectionId, UserId subscriberUserId)
    {
        ParentUserId = parentUserId;
        ParentCollectionId = parentCollectionId;
        SubscriberUserId = subscriberUserId;
    }
}

