using DomainServices.DB.Queries.Store.CollectionPublications;
using DomainServices.DB.Queries.Store.PublicCollection;
using DomainServices.DB.Queries.Store.PublicCollectionSubscribers;
using DomainServices.DB.Repositories;

namespace DomainServices.DB.Queries.Store;

public interface IStoreQueryRepository : IBoundedContextQueryRepository
{
    public ICollectionPublicationQueryResolver Publications { get; } 
    public IPublicCollectionQueryResolver Collections { get; }
    public IPublicCollectionSubscriberQueryResolver Subscribers { get; }
}