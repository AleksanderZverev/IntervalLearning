using DB.Models.ValueObjects;
using Domain.Collection;
using Domain.Collection.ValueObjects;
using Domain.User.ValueObjects;

namespace Application.Common.Interfaces.DB.Queries.Store.PublicCollection;

public interface IPublicCollectionQueryResolver
{
    Task<List<Collection>> Search(ThemeId themeId, string searchName, int skip, int take);
    Task<Collection?> Find(UserId userId, CollectionId collectionId);
}