using Application.Common.Interfaces.Domain.Collections;
using Domain.Collection;
using Domain.Collection.ValueObjects;
using Domain.User.ValueObjects;

namespace DB.Resolvers.Collections;

public class CollectionQueryResolver : ICollectionQueryResolver
{
    private readonly ApplicationContext db;

    public CollectionQueryResolver(ApplicationContext db)
    {
        this.db = db;
    }

    public Task<Collection?> Find(UserId userId, CollectionId collectionId)
    {
        return db.Collections.FindAsync(userId, collectionId).AsTask();
    }
}