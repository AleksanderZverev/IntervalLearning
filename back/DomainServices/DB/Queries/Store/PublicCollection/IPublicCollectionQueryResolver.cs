using Domain.Collection;
using Domain.Collection.ValueObjects;
using Domain.Theme.ValueObjects;
using Domain.User.ValueObjects;

namespace DomainServices.DB.Queries.Store.PublicCollection;

public interface IPublicCollectionQueryResolver
{
    Task<List<Collection>> Search(ThemeId themeId, string searchName, int skip, int take);
    Task<Collection?> Find(UserId userId, CollectionId collectionId);
}