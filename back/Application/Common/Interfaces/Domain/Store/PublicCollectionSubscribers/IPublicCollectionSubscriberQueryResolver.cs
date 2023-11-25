using DB.Models.Store;
using Domain.Collection.ValueObjects;
using Domain.User.ValueObjects;

namespace Application.Common.Interfaces.Domain.Store.PublicCollectionSubscribers;

public interface IPublicCollectionSubscriberQueryResolver
{
    Task<PublicCollectionSubscriber?> FindAsync(UserId userId, CollectionId collectionId, UserId subscriberUserId);
}