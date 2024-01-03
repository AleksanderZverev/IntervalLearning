using DomainServices.DB.Queries.Store;
using DomainServices.DB.Queries.Store.CollectionPublications;
using DomainServices.DB.Queries.Store.PublicCollection;
using DomainServices.DB.Queries.Store.PublicCollectionSubscribers;

namespace DB.Quaries.Store;

public class StoreQueryRepository : IStoreQueryRepository
{
    public ICollectionPublicationQueryResolver Publications { get; }
    public IPublicCollectionQueryResolver Collections { get; }
    public IPublicCollectionSubscriberQueryResolver Subscribers { get; }

    public StoreQueryRepository(
        ICollectionPublicationQueryResolver collectionPublications,
        IPublicCollectionQueryResolver publicCollections,
        IPublicCollectionSubscriberQueryResolver publicCollectionSubscribers)
    {
        Publications = collectionPublications;
        Collections = publicCollections;
        Subscribers = publicCollectionSubscribers;
    }
}