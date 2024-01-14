using Domain.Collection.ValueObjects;
using Domain.Deprecated.DbModels;
using Domain.User.ValueObjects;
using DomainServices.DB.Queries.Store.CollectionPublications;
using Microsoft.EntityFrameworkCore;

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

    public Task<List<CollectionPublicationEntity>> GetAll(UserId userId, CollectionId collectionId)
    {
        return db.CollectionPublications
            .Where(p => p.ParentUserId == userId && p.ParentCollectionId == collectionId)
            .ToListAsync();
    }
}