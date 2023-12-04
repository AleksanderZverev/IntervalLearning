using Application.Common.Interfaces.DB.Queries.Store;
using Application.Common.Interfaces.DB.Repositories;
using Application.Common.Interfaces.DB.Repositories.Store;
using Application.Common.Interfaces.DB.Repositories.Store.PublicCollections;
using Domain.Deprecated.DbModels;
using FluentResults;
using Infrastructure.Errors;

namespace DB.Repository.Store;

public class StoreRepository : IStoreRepository
{
    private readonly ApplicationContext db;
    
    public IStoreQueryRepository Query { get; }
    public IRepository<CollectionPublicationEntity> Publications { get; }

    public IRepository<PublicCollectionSubscriber> Subscribers { get; }
    public IPublicCollectionRepository Collections { get; }

    public StoreRepository(
        ApplicationContext db,
        IStoreQueryRepository query,
        IRepository<CollectionPublicationEntity> collectionPublications, 
        IRepository<PublicCollectionSubscriber> publicCollectionSubscribers,
        IPublicCollectionRepository publicCollectionRepository)
    {
        this.db = db;
        Publications = collectionPublications;
        Subscribers = publicCollectionSubscribers;
        Collections = publicCollectionRepository;
        Query = query;
    }
    
    public Result SaveChanges()
    {
        return Result.OkIf(db.SoftSaveChanges(), new InternalError());
    }

    public async Task<Result> SaveChangesAsync()
    {
        return Result.OkIf(await db.SoftSaveChangesAsync(), new InternalError());
    }
}