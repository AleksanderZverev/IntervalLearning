using Application.Common.Interfaces.Domain.Collections;
using Domain.Collection;
using Domain.Collection.ValueObjects;
using FluentResults;

namespace Application.Commands.Collections.CreateCollection;

public class CreateCollectionCommand : ICommand<CreateCollectionRequest, Collection>
{
    private readonly ICollectionMutationResolver collectionMutationResolver;

    public CreateCollectionCommand(
        ICollectionMutationResolver collectionMutationResolver)
    {
        this.collectionMutationResolver = collectionMutationResolver;
    }

    public Task<Result<Collection>> Handle(CreateCollectionRequest request)
    {
        var collectionId = collectionMutationResolver.GetUniqueId(request.ParentUserId).Value;

        var newCollectionResult = Collection.Create(
            request.ParentUserId,
            collectionId,
            CollectionTitle.Create(request.Title).Value,
            request.ThemeId);

        if (newCollectionResult.IsFailed)
            return Task.FromResult<Result<Collection>>(new Error("Creation error"));

        var newCollection = newCollectionResult.Value;
        newCollection.IsDefaultBackSide = request.IsDefaultBackSide;
        
        return Task.FromResult(collectionMutationResolver.Add(newCollection));
    }
}