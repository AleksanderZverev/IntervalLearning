using Application.Common.Interfaces.DB.Queries.Store;
using Application.Common.Interfaces.Domain.Store.CollectionPublications;
using Application.Common.Interfaces.Domain.Store.PublicCollection;
using Application.Common.Interfaces.Domain.Store.PublicCollectionSubscribers;

namespace DB.Quaries.Store;

public class StoreQueryRepository : IStoreQueryRepository
{
    public ICollectionPublicationQueryResolver CollectionPublications { get; }
    public IPublicCollectionQueryResolver PublicCollections { get; }
    public IPublicCollectionSubscriberQueryResolver PublicCollectionSubscribers { get; }

    public StoreQueryRepository(
        ICollectionPublicationQueryResolver collectionPublications,
        IPublicCollectionQueryResolver publicCollections,
        IPublicCollectionSubscriberQueryResolver publicCollectionSubscribers)
    {
        CollectionPublications = collectionPublications;
        PublicCollections = publicCollections;
        PublicCollectionSubscribers = publicCollectionSubscribers;
    }
}