using Application.Common.Interfaces.DB.Queries.Store;
using Application.Common.Interfaces.DB.Repositories;
using Application.Common.Interfaces.DB.Repositories.Store;
using Application.Common.Interfaces.DB.Repositories.Store.PublicCollections;
using DB.Models.Store;

namespace DB.Repository.Store;

public class StoreRepository : IStoreRepository
{
    public IStoreQueryRepository Query { get; }
    public IRepository<CollectionPublicationEntity> CollectionPublications { get; }

    public IRepository<PublicCollectionSubscriber> CollectionSubscribers { get; }
    public IPublicCollectionRepository PublicCollectionRepository { get; }

    public StoreRepository(
        IStoreQueryRepository query,
        IRepository<CollectionPublicationEntity> collectionPublications, 
        IRepository<PublicCollectionSubscriber> publicCollectionSubscribers,
        IPublicCollectionRepository publicCollectionRepository)
    {
        CollectionPublications = collectionPublications;
        CollectionSubscribers = publicCollectionSubscribers;
        PublicCollectionRepository = publicCollectionRepository;
        Query = query;
    }
}