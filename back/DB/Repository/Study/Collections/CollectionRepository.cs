using Application.Common.Interfaces.DB.Repositories;
using Application.Common.Interfaces.DB.Repositories.Study.Collections;
using DB.Configurations.Study;
using Domain.Collection;
using Domain.Collection.ValueObjects;
using FluentResults;

namespace DB.Repository.Study.Collections;

internal class CollectionRepository : BaseRepository<Collection>, IRepository<Collection, CollectionId, CollectionIdParams>
{
    public CollectionRepository(ApplicationContext db) : base(db)
    {
    }

    public Result<CollectionId> GetUniqueId(CollectionIdParams param)
    {
        var sequenceName = CollectionConfiguration.GetSequenceName(param.UserId);
        const int collectionsStartValue = 100;
        db.EnsureSequenceCreated(sequenceName, collectionsStartValue);
        var collectionNextId = db.GetSequenceNextValue16(sequenceName, collectionsStartValue);
        return CollectionId.Create(collectionNextId);
    }
}