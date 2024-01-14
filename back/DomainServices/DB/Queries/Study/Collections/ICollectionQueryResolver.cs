using Domain.Collection;
using Domain.Collection.ValueObjects;
using Domain.Theme.ValueObjects;
using Domain.User.ValueObjects;
using FluentResults;

namespace DomainServices.DB.Queries.Study.Collections;

public interface ICollectionQueryResolver
{
    Task<Collection?> Find(UserId userId, CollectionId collectionId);
    Task<Result<List<Collection>>> Search(UserId userId, ThemeId themeId, string searchName, int skip, int take);
    Task<List<Collection>> GetAll(UserId userId);
    Task<List<Collection>> GetRange(UserId userId, List<CollectionId> collectionIds);
}