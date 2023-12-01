using Application.Common.Interfaces.DB.Queries.Study;
using Domain.Collection;
using FluentResults;
using Infrastructure.Extensions;

namespace Application.Commands.Collections.GetAllUserCollections;

public class GetAllUserCollectionsCommand : ICommand<GetAllUserCollectionsRequest, List<Collection>>
{
    private readonly IStudyQueryRepository studyQueryRepository;

    public GetAllUserCollectionsCommand(
        IStudyQueryRepository studyQueryRepository)
    {
        this.studyQueryRepository = studyQueryRepository;
    }

    public Task<Result<List<Collection>>> Handle(GetAllUserCollectionsRequest userCollectionsRequest)
    {
        return studyQueryRepository.Collections.GetAll(userCollectionsRequest.UserId).ToResultAsync();
    }
}