using Domain.Collection.ValueObjects;
using Domain.Deprecated.DbModels;
using Domain.User.ValueObjects;

namespace DomainServices.DB.Queries.Store.CollectionPublications;

public interface ICollectionPublicationQueryResolver
{
    public Task<CollectionPublicationEntity?> Find(UserId userId, CollectionId collectionId);
}