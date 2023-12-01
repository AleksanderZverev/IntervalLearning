using Application.Common.Interfaces.DB.Repositories.Study;
using Domain.Collection;
using FluentResults;
using Infrastructure.Errors;
using Infrastructure.Extensions;

namespace Application.Commands.Collections.GetCollection;

public class GetCollectionCommand : ICommand<GetCollectionRequest, Collection>
{
    private readonly IStudyQueryRepository studyQueryRepository;

    public GetCollectionCommand(
        IStudyQueryRepository studyQueryRepository)
    {
        this.studyQueryRepository = studyQueryRepository;
    }

    public Task<Result<Collection>> Handle(GetCollectionRequest request)
    {
        return studyQueryRepository.Collections
            .Find(request.UserId, request.CollectionId)
            .ToResultAsync()
            .ErrorIfNull(new NotFoundError("Collection"));
    }
}