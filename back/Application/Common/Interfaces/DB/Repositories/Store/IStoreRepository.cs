using Application.Common.Interfaces.DB.Queries.Store;
using Application.Common.Interfaces.DB.Repositories.Store.PublicCollections;
using Domain.Deprecated.DbModels;

namespace Application.Common.Interfaces.DB.Repositories.Store;

public interface IStoreRepository : IBoundedContextRepository
{
    public IStoreQueryRepository Query { get; }
    public IRepository<CollectionPublicationEntity> Publications { get; }
    public IRepository<PublicCollectionSubscriber> Subscribers { get; }
    public IPublicCollectionRepository Collections { get; }
}