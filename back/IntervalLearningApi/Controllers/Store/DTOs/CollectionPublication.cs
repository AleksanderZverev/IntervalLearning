using Domain.Deprecated.DbModels;
using Mapster;

namespace IntervalLearningApi.Controllers.Store.DTOs;

public class CollectionPublicationRegister : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<CollectionPublicationEntity, CollectionPublication>();
    }
}

public class CollectionPublication
{
    public DateOnly PublishDate { get; set; }
    public uint SubscribersCount { get; set; }
    public uint LikesCount { get; set; }
    public uint DislikesCount { get; set; }
}