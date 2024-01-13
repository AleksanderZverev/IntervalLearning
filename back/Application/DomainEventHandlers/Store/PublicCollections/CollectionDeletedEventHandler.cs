using Domain.Collection.Events;
using Domain.Collection.ValueObjects;
using Domain.User.ValueObjects;
using DomainServices.DB.Repositories.Store;
using FluentResults;

namespace Application.DomainEventHandlers.Store.PublicCollections;

public class CollectionDeletedEventHandler : IDomainEventHandler<CollectionDeletedEvent>
{
    private readonly IStoreRepository storeRepository;

    public CollectionDeletedEventHandler(IStoreRepository storeRepository)
    {
        this.storeRepository = storeRepository;
    }

    public async Task<Result> Handle(CollectionDeletedEvent collectionDeletedEvent)
    {
        var (userId, collectionId) = collectionDeletedEvent;

        var publicCollection = await storeRepository.Query.Collections.Find(userId, collectionId);
        
        if (publicCollection == null)
            return Result.Ok();

        return Result.Merge(
            await DeletePublications(userId, collectionId),
            await DeleteSubscribers(userId, collectionId)
        );
    }

    private async Task<Result> DeletePublications(UserId userId, CollectionId collectionId)
    {
        var publications = await storeRepository.Query.Publications.GetAll(userId, collectionId);
        
        if (publications is not {Count: > 0})
            return Result.Ok();

        storeRepository.Publications.DeleteRange(publications);
        return Result.Ok();
    }

    private async Task<Result> DeleteSubscribers(UserId userId, CollectionId collectionId)
    {
        var subscribers = await storeRepository.Query.Subscribers.GetAll(userId, collectionId);
        
        if (subscribers is not {Count: > 0})
            return Result.Ok();

        storeRepository.Subscribers.DeleteRange(subscribers);
        return Result.Ok();
    }
}