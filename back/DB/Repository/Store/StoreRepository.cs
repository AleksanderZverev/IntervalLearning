using Application.Common.Interfaces.DB.Repositories;
using Application.Common.Interfaces.DB.Repositories.Store;
using Application.Common.Interfaces.DB.Repositories.Store.PublicCollections;
using DB.Models.Store;

namespace DB.Repository.Store;

public class StoreRepository : IStoreRepository
{
    public IRepository<CollectionPublicationEntity> CollectionPublications { get; }

    public IRepository<PublicCollectionSubscriber> CollectionSubscribers { get; }
    public IPublicCollectionRepository PublicCollectionRepository { get; }

    public StoreRepository(
        IRepository<CollectionPublicationEntity> collectionPublications, 
        IRepository<PublicCollectionSubscriber> publicCollectionSubscribers,
        IPublicCollectionRepository publicCollectionRepository)
    {
        CollectionPublications = collectionPublications;
        CollectionSubscribers = publicCollectionSubscribers;
        PublicCollectionRepository = publicCollectionRepository;
    }
}