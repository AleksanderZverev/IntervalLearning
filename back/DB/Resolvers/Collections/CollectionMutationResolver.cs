using Application.Common.Interfaces.Domain.Collections;
using Domain.Collection;
using FluentResults;
using Infrastructure.Errors;

namespace DB.Resolvers.Collections;

public class CollectionMutationResolver : ICollectionMutationResolver
{
    private readonly ApplicationContext db;

    public CollectionMutationResolver(ApplicationContext db)
    {
        this.db = db;
    }

    public Result<Collection> Add(Collection entity)
    {
        var entry = db.Collections.Add(entity);

        if (!db.SoftSaveChanges())
        {
            return new InternalError();
        }

        return entity;
    }

    public Result<Collection> Update(Collection entity)
    {
        var entry = db.Collections.Update(entity);

        if (!db.SoftSaveChanges())
        {
            return new InternalError();
        }

        return entity;
    }

    public Result<Collection> Delete(Collection entity)
    {
        var entry = db.Collections.Remove(entity);

        if (!db.SoftSaveChanges())
        {
            return new InternalError();
        }

        return entity;
    }
}