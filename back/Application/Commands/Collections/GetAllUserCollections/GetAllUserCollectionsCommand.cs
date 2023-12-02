using Application.Common.Interfaces.DB.Queries.Study;
using Domain.Collection;
using FluentResults;
using Infrastructure.Extensions;

namespace Application.Commands.Collections.GetAllUserCollections;

public class GetAllUserCollectionsCommand : ICommand<GetAllUserCollectionsCommandRequest, List<Collection>>
{
    private readonly IStudyQueryRepository studyQueryRepository;

    public GetAllUserCollectionsCommand(
        IStudyQueryRepository studyQueryRepository)
    {
        this.studyQueryRepository = studyQueryRepository;
    }

    public Task<Result<List<Collection>>> Handle(GetAllUserCollectionsCommandRequest userCollectionsCommandRequest)
    {
        return studyQueryRepository.Collections.GetAll(userCollectionsCommandRequest.UserId).ToResultAsync();
    }
}