using Application.Common.Interfaces.DB.Queries.Store.CollectionPublications;
using Application.Common.Interfaces.DB.Queries.Store.PublicCollection;
using Application.Common.Interfaces.DB.Queries.Store.PublicCollectionSubscribers;

namespace Application.Common.Interfaces.DB.Queries.Store;

public interface IStoreQueryRepository
{
    public ICollectionPublicationQueryResolver Publications { get; } 
    public IPublicCollectionQueryResolver Collections { get; }
    public IPublicCollectionSubscriberQueryResolver Subscribers { get; }
}