using DB.Models;
using DB.Models.Store;
using Domain.Collection;
using Mapster;

namespace IntervalLearningApi.Models.ByUser;

public class StoreCollectionRegister : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<(Collection, PublicCollectionSubscriber?), StoreCollection>()
            .Map(d => d, s => s.Item1)
            .Map(d => d.CreatedAt, s => s.Item1.CreatedDate)
            .Map(d => d.NotStartedCards, s => s.Item1.NotStartedCardsCount)
            .Map(d => d.OwnerUser, s => s.Item1.ParentUser)
            .Map(d => d.Publication, s => s.Item1.CollectionPublicationEntity)
            .Map(d => d, s => s.Item2)
            .IgnoreIf((s, d) => s.Item2 == null, d => d.IsAdded, d => d.IsDisliked, d => d.IsLiked);
    }
}

public class StoreCollection : CollectionDto
{
    public UserInfo OwnerUser { get; set; }
    public bool IsLiked { get; set; }
    public bool IsDisliked { get; set; }
    public bool IsAdded { get; set; }
}