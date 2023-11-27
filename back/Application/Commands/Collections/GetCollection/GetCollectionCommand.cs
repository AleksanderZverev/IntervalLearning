using Application.Common.Interfaces.Domain.Collections;
using Domain.Collection;
using FluentResults;
using Infrastructure.Errors;
using Infrastructure.Extensions;

namespace Application.Commands.Collections.GetCollection;

public class GetCollectionCommand : ICommand<GetCollectionRequest, Collection>
{
    private readonly ICollectionQueryResolver collectionQueryResolver;

    public GetCollectionCommand(
        ICollectionQueryResolver collectionQueryResolver)
    {
        this.collectionQueryResolver = collectionQueryResolver;
    }

    public Task<Result<Collection>> Handle(GetCollectionRequest request)
    {
        return collectionQueryResolver
            .Find(request.UserId, request.CollectionId)
            .ToResultAsync()
            .ErrorIfNull(new NotFoundError("Collection"));
    }
}