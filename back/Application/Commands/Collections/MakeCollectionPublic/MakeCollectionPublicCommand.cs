using Application.Common.Interfaces.DB.Queries.Study;
using Application.Common.Interfaces.DB.Repositories.Store;
using Application.Common.Interfaces.DB.Repositories.Study;
using Application.Common.Interfaces.DB.Transactions;
using DB.Models.Store;
using Domain.Collection;
using FluentResults;
using FluentResults.Extensions;
using Infrastructure.Errors;
using Infrastructure.Extensions;

namespace Application.Commands.Collections.MakeCollectionPublic;

public class MakeCollectionPublicCommand : ICommand<MakeCollectionPublicRequest, Collection>
{
    private readonly IStudyQueryRepository studyQueryRepository; 
    private readonly IStoreRepository storeRepository;
    private readonly ITransactionProvider transactionProvider;

    public MakeCollectionPublicCommand(
        IStoreRepository storeRepository,
        ITransactionProvider transactionProvider, 
        IStudyQueryRepository studyQueryRepository)
    {
        this.storeRepository = storeRepository;
        this.transactionProvider = transactionProvider;
        this.studyQueryRepository = studyQueryRepository;
    }

    public async Task<Result<Collection>> Handle(MakeCollectionPublicRequest request)
    {
        var (userId, collectionId) = request;
        return await studyQueryRepository.Collections
            .Find(userId, collectionId)
            .ToResultAsync()
            .ErrorIfNull(new NotFoundError("Collection"))
            .Bind(async collection =>
            {
                var existingPublication = await storeRepository.Query.Publications.Find(userId, collectionId);

                if (existingPublication != null)
                    return new BadRequestError("Collection is already published");

                using var transaction = transactionProvider.CreateScope();
                var publication = new CollectionPublicationEntity()
                {
                    ParentUserId = userId,
                    ParentCollectionId = collectionId,
                };

                var addedPublicationResult = storeRepository.Publications.Add(publication);

                if (addedPublicationResult.IsFailed)
                {
                    return new InternalError();
                }

                collection.MakePublic();
                var updateResult = storeRepository.Collections.Update(collection);
        
                if (updateResult.IsFailed)
                {
                    return new InternalError();
                }

                transaction.Complete();
                return collection.ToResult();
            });
    }
}