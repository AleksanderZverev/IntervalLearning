using Application.Common.Interfaces.DB.Repositories.Store.PublicCollections;
using Application.Common.Interfaces.DB.Repositories.Study;
using Domain.Collection;
using FluentResults;

namespace DB.Repository.Store.PublicCollections;

public class PublicCollectionRepository : IPublicCollectionRepository
{
    private readonly IStudyRepository studyRepository;

    public PublicCollectionRepository(IStudyRepository studyRepository)
    {
        this.studyRepository = studyRepository;
    }

    public Result<Collection> Update(Collection collection)
    {
        studyRepository.Collections.Update(collection);
        return collection;
    }
}