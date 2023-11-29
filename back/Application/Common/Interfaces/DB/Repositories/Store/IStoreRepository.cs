using Application.Common.Interfaces.DB.Queries.Store;
using Application.Common.Interfaces.DB.Repositories.Store.PublicCollections;
using DB.Models.Store;

namespace Application.Common.Interfaces.DB.Repositories.Store;

public interface IStoreRepository : IBoundedContextRepository
{
    public IStoreQueryRepository Query { get; }
    public IRepository<CollectionPublicationEntity> CollectionPublications { get; }
    public IRepository<PublicCollectionSubscriber> CollectionSubscribers { get; }
    public IPublicCollectionRepository PublicCollectionRepository { get; }
}