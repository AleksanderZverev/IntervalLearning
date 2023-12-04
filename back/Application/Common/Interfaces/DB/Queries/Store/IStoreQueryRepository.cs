using Application.Common.Interfaces.DB.Queries.Store.CollectionPublications;
using Application.Common.Interfaces.DB.Queries.Store.PublicCollection;
using Application.Common.Interfaces.DB.Queries.Store.PublicCollectionSubscribers;
using Application.Common.Interfaces.DB.Repositories;

namespace Application.Common.Interfaces.DB.Queries.Store;

public interface IStoreQueryRepository : IBoundedContextQueryRepository
{
    public ICollectionPublicationQueryResolver Publications { get; } 
    public IPublicCollectionQueryResolver Collections { get; }
    public IPublicCollectionSubscriberQueryResolver Subscribers { get; }
}