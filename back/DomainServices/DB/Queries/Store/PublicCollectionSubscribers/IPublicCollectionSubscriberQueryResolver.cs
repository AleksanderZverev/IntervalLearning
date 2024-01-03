using Domain.Collection.ValueObjects;
using Domain.Deprecated.DbModels;
using Domain.User.ValueObjects;

namespace DomainServices.DB.Queries.Store.PublicCollectionSubscribers;

public interface IPublicCollectionSubscriberQueryResolver
{
    Task<PublicCollectionSubscriber?> Find(UserId userId, CollectionId collectionId, UserId subscriberUserId);
}