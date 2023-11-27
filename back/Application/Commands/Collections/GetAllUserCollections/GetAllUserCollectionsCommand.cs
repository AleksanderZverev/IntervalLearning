using Application.Common.Interfaces.Domain.Collections;
using Domain.Collection;
using FluentResults;
using Infrastructure.Extensions;

namespace Application.Commands.Collections.GetAll;

public class GetAllUserCollectionsCommand : ICommand<GetAllUserCollectionsRequest, List<Collection>>
{
    private readonly ICollectionQueryResolver collectionQueryResolver;

    public GetAllUserCollectionsCommand(ICollectionQueryResolver collectionQueryResolver)
    {
        this.collectionQueryResolver = collectionQueryResolver;
    }

    public Task<Result<List<Collection>>> Handle(GetAllUserCollectionsRequest userCollectionsRequest)
    {
        return collectionQueryResolver.GetAll(userCollectionsRequest.UserId).ToResultAsync();
    }
}