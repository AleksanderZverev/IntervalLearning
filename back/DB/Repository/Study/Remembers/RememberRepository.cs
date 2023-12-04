using Application.Common.Interfaces.DB.Repositories;
using Application.Common.Interfaces.DB.Repositories.Study.CardRemembers;
using DB.Configurations.Study;
using DB.Repository;
using Domain.Card.ValueObjects;
using Domain.Schedule.Entities.Remember.ValueObjects;
using Domain.Schedule.ValueObjects;
using FluentResults;

namespace DB.Resolvers.Study.Remember;

internal class RememberRepository : BaseRepository<Domain.Schedule.Entities.Remember.Remember>, IRepository<Domain.Schedule.Entities.Remember.Remember, RememberId, RememberIdParams>
{
    public RememberRepository(ApplicationContext db) : base(db)
    {
    }

    public Result<RememberId> GetUniqueId(RememberIdParams param)
    {
        var (schedule, card) = param;
        var sequenceName = RememberConfiguration.GetSequenceName(
            new ComplexScheduleId()
            {
                ParentUserId = schedule.ParentUserId,
                Id = schedule.Id,
            },
            new ComplexCardId()
            {
                UserId = card.ParentUserId,
                CollectionId = card.ParentCollectionId,
                Id = card.Id,
            });
        
        db.EnsureSequenceCreated(sequenceName);
        var nextValue = db.GetSequenceNextValue16(sequenceName);
        return RememberId.Create(nextValue);
    }
}