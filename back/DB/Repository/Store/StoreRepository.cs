using Application.Common.Interfaces.DB.Queries.Store;
using Application.Common.Interfaces.DB.Repositories;
using Application.Common.Interfaces.DB.Repositories.Store;
using Application.Common.Interfaces.DB.Repositories.Store.PublicCollections;
using DB.Models.Store;

namespace DB.Repository.Store;

public class StoreRepository : IStoreRepository
{
    public IStoreQueryRepository Query { get; }
    public IRepository<CollectionPublicationEntity> Publications { get; }

    public IRepository<PublicCollectionSubscriber> Subscribers { get; }
    public IPublicCollectionRepository Collections { get; }

    public StoreRepository(
        IStoreQueryRepository query,
        IRepository<CollectionPublicationEntity> collectionPublications, 
        IRepository<PublicCollectionSubscriber> publicCollectionSubscribers,
        IPublicCollectionRepository publicCollectionRepository)
    {
        Publications = collectionPublications;
        Subscribers = publicCollectionSubscribers;
        Collections = publicCollectionRepository;
        Query = query;
    }
}