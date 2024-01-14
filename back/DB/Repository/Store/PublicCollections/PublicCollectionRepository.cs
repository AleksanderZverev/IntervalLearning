using Domain.Collection;
using DomainServices.DB.Repositories.Store.PublicCollections;
using DomainServices.DB.Repositories.Study;

namespace DB.Repository.Store.PublicCollections;

public class PublicCollectionRepository : IPublicCollectionRepository
{
    private readonly IStudyRepository studyRepository;

    public PublicCollectionRepository(IStudyRepository studyRepository)
    {
        this.studyRepository = studyRepository;
    }

    public Collection Update(Collection collection)
    {
        return studyRepository.Collections.Update(collection);
    }
}