using Domain.Collection;

namespace DomainServices.DB.Repositories.Store.PublicCollections;

public interface IPublicCollectionRepository
{
    public Collection Update(Collection collection);
}