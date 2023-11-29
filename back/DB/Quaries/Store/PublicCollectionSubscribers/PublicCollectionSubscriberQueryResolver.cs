using Application.Common.Interfaces.Domain.Store.PublicCollectionSubscribers;
using DB.Models.Store;
using Domain.Collection.ValueObjects;
using Domain.User.ValueObjects;

namespace DB.Resolvers.Store.PublicCollectionSubscribers;

public class PublicCollectionSubscriberQueryResolver : IPublicCollectionSubscriberQueryResolver
{
    private readonly ApplicationContext db;

    public PublicCollectionSubscriberQueryResolver(ApplicationContext db)
    {
        this.db = db;
    }

    public Task<PublicCollectionSubscriber?> Find(UserId userId, CollectionId collectionId, UserId subscriberUserId)
    {
        return db.PublicCollectionSubscribers.FindAsync(userId, collectionId, subscriberUserId).AsTask();
    }
}