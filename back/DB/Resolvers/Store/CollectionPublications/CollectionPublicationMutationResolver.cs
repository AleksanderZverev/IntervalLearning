using Application.Common.Interfaces.Domain.Store.CollectionPublications;
using DB.Models.Store;

namespace DB.Resolvers.Store.CollectionPublications;

public class CollectionPublicationMutationResolver : BaseMutationResolver<CollectionPublicationEntity>, ICollectionPublicationMutationResolver
{
    public CollectionPublicationMutationResolver(ApplicationContext db) : base(db)
    {
    }

    protected override void MarkAdded(CollectionPublicationEntity entity)
    {
        db.CollectionPublications.Add(entity);
    }

    protected override void MarkUpdated(CollectionPublicationEntity entity)
    {
        db.CollectionPublications.Update(entity);
    }

    protected override void MarkRemoved(CollectionPublicationEntity entity)
    {
        db.CollectionPublications.Remove(entity);
    }
}