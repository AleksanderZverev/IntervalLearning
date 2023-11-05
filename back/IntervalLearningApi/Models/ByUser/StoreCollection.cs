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
    public UserInfo OwnerUser { get; set; }
    public bool IsLiked { get; set; }
    public bool IsDisliked { get; set; }
    public bool IsAdded { get; set; }
}