using Application.Common.Interfaces.DB.Transactions;
using Application.Common.Interfaces.Domain.Collections;
using Application.Common.Interfaces.Domain.Store.CollectionPublications;
using Application.Common.Interfaces.Domain.Store.PublicCollection;
using DB.Models.Store;
using Domain.Collection;
using FluentResults;
using FluentResults.Extensions;
using Infrastructure.Errors;
using Infrastructure.Extensions;

namespace Application.Commands.Collections.MakeCollectionPublic;

public class MakeCollectionPublicCommand : ICommand<MakeCollectionPublicRequest, Collection>
{
    private readonly ICollectionQueryResolver collectionQueryResolver;
    private readonly ICollectionMutationResolver collectionMutationResolver;
    private readonly IPublicCollectionQueryResolver publicCollectionQueryResolver;
    private readonly ICollectionPublicationQueryResolver collectionPublicationQueryResolver;
    private readonly ICollectionPublicationMutationResolver collectionPublicationMutationResolver;
    private readonly ITransactionProvider transactionProvider;

    public MakeCollectionPublicCommand(
        ICollectionQueryResolver collectionQueryResolver,
        ICollectionMutationResolver collectionMutationResolver,
        IPublicCollectionQueryResolver publicCollectionQueryResolver,
        ICollectionPublicationQueryResolver collectionPublicationQueryResolver,
        ICollectionPublicationMutationResolver collectionPublicationMutationResolver,
        ITransactionProvider transactionProvider)
    {
        this.collectionQueryResolver = collectionQueryResolver;
        this.collectionMutationResolver = collectionMutationResolver;
        this.publicCollectionQueryResolver = publicCollectionQueryResolver;
        this.collectionPublicationQueryResolver = collectionPublicationQueryResolver;
        this.collectionPublicationMutationResolver = collectionPublicationMutationResolver;
        this.transactionProvider = transactionProvider;
    }

    public async Task<Result<Collection>> Handle(MakeCollectionPublicRequest request)
    {
        var (userId, collectionId) = request;
        return await collectionQueryResolver
            .Find(userId, collectionId)
            .ToResultAsync()
            .ErrorIfNull(new NotFoundError("Collection"))
            .Bind(async collection =>
            {
                var existingPublication = await collectionPublicationQueryResolver.Find(userId, collectionId);

                if (existingPublication != null)
                    return new BadRequestError("Collection is already published");

                using var transaction = transactionProvider.CreateScope();
                var publication = new CollectionPublicationEntity()
                {
                    ParentUserId = userId,
                    ParentCollectionId = collectionId,
                };

                var addedPublicationResult = collectionPublicationMutationResolver.Add(publication);

                if (addedPublicationResult.IsFailed)
                {
                    return new InternalError();
                }

                collection.IsPublic = true;
                var updateResult = collectionMutationResolver.Update(collection);
        
                if (updateResult.IsFailed)
                {
                    return new InternalError();
                }

                transaction.Complete();
                return collection.ToResult();
            });
    }
}