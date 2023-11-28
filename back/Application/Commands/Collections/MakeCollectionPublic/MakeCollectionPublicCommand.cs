using Application.Common.Interfaces.DB.Repositories.Store;
using Application.Common.Interfaces.DB.Transactions;
using Application.Common.Interfaces.Domain.Collections;
using Application.Common.Interfaces.Domain.Store.CollectionPublications;
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
    private readonly ICollectionPublicationQueryResolver collectionPublicationQueryResolver;
    private readonly IStoreRepository storeRepository;
    private readonly ITransactionProvider transactionProvider;

    public MakeCollectionPublicCommand(
        ICollectionQueryResolver collectionQueryResolver,
        ICollectionPublicationQueryResolver collectionPublicationQueryResolver,
        IStoreRepository storeRepository,
        ITransactionProvider transactionProvider)
    {
        this.collectionQueryResolver = collectionQueryResolver;
        this.collectionPublicationQueryResolver = collectionPublicationQueryResolver;
        this.storeRepository = storeRepository;
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

                var addedPublicationResult = storeRepository.CollectionPublications.Add(publication);

                if (addedPublicationResult.IsFailed)
                {
                    return new InternalError();
                }

                collection.MakePublic();
                var updateResult = storeRepository.PublicCollectionRepository.Update(collection);
        
                if (updateResult.IsFailed)
                {
                    return new InternalError();
                }

                transaction.Complete();
                return collection.ToResult();
            });
    }
}