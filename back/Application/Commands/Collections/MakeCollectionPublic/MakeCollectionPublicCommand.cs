using Domain.Collection;
using Domain.Deprecated.DbModels;
using DomainServices.DB.Queries.Study;
using DomainServices.DB.Repositories.Store;
using DomainServices.DB.Transactions;
using FluentResults;
using FluentResults.Extensions;
using GlobalTools.Errors;
using GlobalTools.Extensions;

namespace Application.Commands.Collections.MakeCollectionPublic;

public class MakeCollectionPublicCommand : ICommand<MakeCollectionPublicCommandRequest, Collection>
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

    public async Task<Result<Collection>> Handle(MakeCollectionPublicCommandRequest request)
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

                storeRepository.Publications.Add(publication);

                collection.MakePublic();
                storeRepository.Collections.Update(collection);

                var updateResult = await storeRepository.SaveChangesAsync();
                
                if (updateResult.IsFailed)
                {
                    return new InternalError();
                }

                transaction.Complete();
                return collection.ToResult();
            });
    }
}