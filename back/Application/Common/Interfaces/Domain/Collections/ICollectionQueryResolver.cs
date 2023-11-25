using DB.Models.ValueObjects;
using Domain.Collection;
using Domain.Collection.ValueObjects;
using Domain.User.ValueObjects;

namespace Application.Common.Interfaces.Domain.Collections;

public interface ICollectionQueryResolver
{
    Task<Collection?> Find(UserId userId, CollectionId collectionId);
    Task<List<Collection>> SearchPublicCollection(ThemeId themeId, string searchName);
}