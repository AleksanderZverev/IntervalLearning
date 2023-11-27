using Application.Common.Interfaces.Domain.Store.PublicCollection;
using Domain.Collection;
using FluentResults;
using Infrastructure.Errors;
using Infrastructure.Extensions;

namespace Application.Commands.Collections.GetPublicCollection;

public class GetPublicCollectionCommand : ICommand<GetPublicCollectionRequest, Collection>
{
    private readonly IPublicCollectionQueryResolver publicCollectionQueryResolver;

    public GetPublicCollectionCommand(
        IPublicCollectionQueryResolver publicCollectionQueryResolver)
    {
        this.publicCollectionQueryResolver = publicCollectionQueryResolver;
    }

    public Task<Result<Collection>> Handle(GetPublicCollectionRequest request)
    {
        return publicCollectionQueryResolver
            .Find(request.UserId, request.CollectionId)
            .ToResultAsync()
            .ErrorIfNull(new NotFoundError("Public collection"));
    }
}