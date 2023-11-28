using Application.Common.Interfaces.Domain.Store.PublicCollectionSubscribers;
using DB.Models.Store;

namespace DB.Resolvers.Store.PublicCollectionSubscribers;

public class PublicCollectionSubscriberMutationResolver : BaseMutationResolver<PublicCollectionSubscriber>,
    IPublicCollectionSubscriberMutationResolver
{
    public PublicCollectionSubscriberMutationResolver(ApplicationContext db) : base(db)
    {
    }

    protected override void MarkAdded(PublicCollectionSubscriber entity)
    {
        db.PublicCollectionSubscribers.Add(entity);
    }

    protected override void MarkUpdated(PublicCollectionSubscriber entity)
    {
        db.PublicCollectionSubscribers.Update(entity);
    }

    protected override void MarkRemoved(PublicCollectionSubscriber entity)
    {
        db.PublicCollectionSubscribers.Remove(entity);
    }
}