using DB.Models;
using DB.Models.Store;
using Mapster;

namespace IntervalLearningApi.Models.ByUser;

public class StoreCollectionRegister : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<(CollectionEntity, PublicCollectionSubscriber), StoreCollection>()
            .Map(d => d, s => s.Item1)
            .Map(d => d.OwnerUser, s => s.Item1.ParentUser)
            .Map(d => d.Publication, s => s.Item1.CollectionPublicationEntity)
            .Map(d => d, s => s.Item2);
    }
}

public class StoreCollection : Collection
{
    public UserInfo OwnerUser { get; }
    public bool IsLiked { get; }
    public bool IsDisliked { get; }
    public bool IsAdded { get; }

    public StoreCollection(
        UserInfo owner,
        long userId,
        short id,
        string title,
        DateTime createdAt,
        short themeId,
        short cardsCount,
        short notStartedCards,
        bool isPublic,
        CollectionPublication? publication, 
        bool isLiked,
        bool isDisliked, 
        bool isAdded)
        : base(
            userId,
            id,
            title,
            createdAt,
            themeId,
            cardsCount,
            notStartedCards,
            isPublic,
            publication)
    {
        OwnerUser = owner;
        IsLiked = isLiked;
        IsDisliked = isDisliked;
        IsAdded = isAdded;
    }
}