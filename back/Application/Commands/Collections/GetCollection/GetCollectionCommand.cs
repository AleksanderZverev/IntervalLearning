using Domain.Collection;
using DomainServices.DB.Queries.Study;
using FluentResults;
using GlobalTools.Errors;
using GlobalTools.Extensions;

namespace Application.Commands.Collections.GetCollection;

public class GetCollectionCommand : ICommand<GetCollectionCommandRequest, Collection>
{
    private readonly IStudyQueryRepository studyQueryRepository;

    public GetCollectionCommand(
        IStudyQueryRepository studyQueryRepository)
    {
        this.studyQueryRepository = studyQueryRepository;
    }

    public Task<Result<Collection>> Handle(GetCollectionCommandRequest request)
    {
        return studyQueryRepository.Collections
            .Find(request.UserId, request.CollectionId)
            .ToResultAsync()
            .ErrorIfNull(new NotFoundError("Collection"));
    }
}