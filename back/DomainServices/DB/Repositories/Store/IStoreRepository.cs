using Domain.Deprecated.DbModels;
using DomainServices.DB.Queries.Store;
using DomainServices.DB.Repositories.Store.PublicCollections;

namespace DomainServices.DB.Repositories.Store;

public interface IStoreRepository : IBoundedContextRepository
{
    public IStoreQueryRepository Query { get; }
    public IRepository<CollectionPublicationEntity> Publications { get; }
    public IRepository<PublicCollectionSubscriber> Subscribers { get; }
    public IPublicCollectionRepository Collections { get; }
}