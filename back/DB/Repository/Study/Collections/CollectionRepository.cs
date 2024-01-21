using DB.Configurations.Study;
using Domain.Collection;
using Domain.Collection.ValueObjects;
using Domain.User.ValueObjects;
using DomainServices.DB.Repositories;
using DomainServices.DB.Repositories.Study.Collections;
using FluentResults;

namespace DB.Repository.Study.Collections;

internal class CollectionRepository : BaseRepository<Collection>, IRepository<Collection, CollectionId, CollectionIdParams>
{
    public CollectionRepository(ApplicationContext db) : base(db)
    {
    }

    private static string GetSequenceName(UserId userId) =>$"collection_for_{userId.Value}";

    public Result<CollectionId> GetUniqueId(CollectionIdParams param)
    {
        var sequenceName = GetSequenceName(param.UserId);
        const int collectionsStartValue = 100;
        db.EnsureSequenceCreated(sequenceName, collectionsStartValue);
        var collectionNextId = db.GetSequenceNextValue16(sequenceName, collectionsStartValue);
        return CollectionId.Create(collectionNextId);
    }
}