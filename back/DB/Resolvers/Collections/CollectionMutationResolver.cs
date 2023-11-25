using Application.Common.Interfaces.Domain.Collections;
using Domain.Collection;

namespace DB.Resolvers.Collections;

public class CollectionMutationResolver : BaseMutationResolver<Collection>, ICollectionMutationResolver
{
    public CollectionMutationResolver(ApplicationContext db) : base(db)
    {
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