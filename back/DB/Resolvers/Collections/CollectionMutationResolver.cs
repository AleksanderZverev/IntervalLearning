using Application.Common.Interfaces.Domain.Collections;
using DB.Configurations.Study;
using Domain.Collection;
using Domain.Collection.ValueObjects;
using Domain.User.ValueObjects;
using FluentResults;

namespace DB.Resolvers.Collections;

public class CollectionMutationResolver : BaseMutationResolver<Collection>, ICollectionMutationResolver
{
    public CollectionMutationResolver(ApplicationContext db) : base(db)
    {
    }

    public Result<CollectionId> GetUniqueId(UserId userId)
    {
        var sequenceName = CollectionConfiguration.GetSequenceName(userId);
        db.EnsureSequenceCreated(sequenceName);
        var collectionNextId = db.GetSequenceNextValue16(sequenceName);
        return CollectionId.Create(collectionNextId);
    }

    protected override void MarkAdded(Collection entity)
    {
        db.Collections.Add(entity);
    }

    protected override void MarkUpdated(Collection entity)
    {
        db.Collections.Update(entity);
    }

    protected override void MarkRemoved(Collection entity)
    {
        db.Collections.Remove(entity);
    }
}