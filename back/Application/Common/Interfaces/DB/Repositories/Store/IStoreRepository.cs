using Application.Common.Interfaces.DB.Repositories.Store.PublicCollections;
using DB.Models.Store;

namespace Application.Common.Interfaces.DB.Repositories.Store;

public interface IStoreRepository : IBoundedContextRepository
{
    public IRepository<CollectionPublicationEntity> CollectionPublications { get; }
    public IRepository<PublicCollectionSubscriber> CollectionSubscribers { get; }
    public IPublicCollectionRepository PublicCollectionRepository { get; }
}