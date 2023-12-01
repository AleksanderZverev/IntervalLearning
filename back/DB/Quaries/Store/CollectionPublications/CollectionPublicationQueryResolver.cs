using Application.Common.Interfaces.DB.Queries.Store.CollectionPublications;
using DB.Models.Store;
using Domain.Collection.ValueObjects;
using Domain.User.ValueObjects;

namespace DB.Resolvers.Store.CollectionPublications;

public class CollectionPublicationQueryResolver : ICollectionPublicationQueryResolver
{
    private readonly ApplicationContext db;

    public CollectionPublicationQueryResolver(ApplicationContext db)
    {
        this.db = db;
    }

    public Task<CollectionPublicationEntity?> Find(UserId userId, CollectionId collectionId)
    {
        return db.CollectionPublications.FindAsync(userId, collectionId).AsTask();
    }
}