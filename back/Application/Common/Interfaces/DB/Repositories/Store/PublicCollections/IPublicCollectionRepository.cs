using Domain.Collection;
using FluentResults;

namespace Application.Common.Interfaces.DB.Repositories.Store.PublicCollections;

public interface IPublicCollectionRepository
{
    public Collection Update(Collection collection);
}