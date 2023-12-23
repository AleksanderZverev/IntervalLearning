using Application.Common.Interfaces.DB.Queries.Store;
using Domain.Collection;
using FluentResults;
using Infrastructure.Errors;
using Infrastructure.Extensions;

namespace Application.Commands.Collections.GetPublicCollection;

public class GetPublicCollectionCommand : ICommand<GetPublicCollectionCommandRequest, Collection>
{
    private readonly IStoreQueryRepository storeQueryRepository;

    public GetPublicCollectionCommand(
        IStoreQueryRepository storeQueryRepository)
    {
        this.storeQueryRepository = storeQueryRepository;
    }

    public Task<Result<Collection>> Handle(GetPublicCollectionCommandRequest request)
    {
        return storeQueryRepository.Collections
            .Find(request.UserId, request.CollectionId)
            .ToResultAsync()
            .ErrorIfNull(new NotFoundError("Public collection"));
    }
}