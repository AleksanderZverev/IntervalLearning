using Domain.Collection.ValueObjects;
using Domain.Deprecated.DbModels;
using Domain.User.ValueObjects;

namespace Application.Common.Interfaces.DB.Queries.Store.CollectionPublications;

public interface ICollectionPublicationQueryResolver
{
    public Task<CollectionPublicationEntity?> Find(UserId userId, CollectionId collectionId);
}