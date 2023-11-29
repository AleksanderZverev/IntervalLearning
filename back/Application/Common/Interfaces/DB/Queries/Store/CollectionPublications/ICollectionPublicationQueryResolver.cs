using DB.Models.Store;
using Domain.Collection.ValueObjects;
using Domain.User.ValueObjects;

namespace Application.Common.Interfaces.Domain.Store.CollectionPublications;

public interface ICollectionPublicationQueryResolver
{
    public Task<CollectionPublicationEntity?> Find(UserId userId, CollectionId collectionId);
}