using Application.Common.Interfaces.DB;
using Domain.Collection;
using Domain.Collection.ValueObjects;
using Domain.User.ValueObjects;
using FluentResults;

namespace Application.Common.Interfaces.Domain.Collections;

public interface ICollectionMutationResolver : IMutationResolver<Collection>
{
    public Result<CollectionId> GetUniqueId(UserId userId);
}