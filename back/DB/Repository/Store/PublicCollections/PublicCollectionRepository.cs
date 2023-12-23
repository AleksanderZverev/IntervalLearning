using Application.Common.Interfaces.DB.Repositories.Store.PublicCollections;
using Application.Common.Interfaces.DB.Repositories.Study;
using Domain.Collection;

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