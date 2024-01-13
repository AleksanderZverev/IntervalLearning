using Domain.Collection.ValueObjects;
using Domain.Deprecated.DbModels;
using Domain.User.ValueObjects;
using DomainServices.DB.Queries.Store.PublicCollectionSubscribers;
using Microsoft.EntityFrameworkCore;

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

    public Task<List<PublicCollectionSubscriber>> GetAll(UserId userId, CollectionId collectionId)
    {
        return db.PublicCollectionSubscribers
            .Where(s => s.ParentUserId == userId && s.ParentCollectionId == collectionId)
            .ToListAsync();
    }
}