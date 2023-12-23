using Application.Common.Interfaces.DB.Queries.Store;
using Application.Common.Interfaces.DB.Queries.Store.CollectionPublications;
using Application.Common.Interfaces.DB.Queries.Store.PublicCollection;
using Application.Common.Interfaces.DB.Queries.Store.PublicCollectionSubscribers;

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