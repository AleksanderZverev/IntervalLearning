using DB.Configurations.Study;
using DB.Repository;
using Domain.Card.ValueObjects;
using Domain.Schedule.Entities.Remember.ValueObjects;
using Domain.Schedule.ValueObjects;
using DomainServices.DB.Repositories;
using DomainServices.DB.Repositories.Study.CardRemembers;
using FluentResults;

namespace DB.Resolvers.Study.Remember;

internal class RememberRepository : BaseRepository<Domain.Schedule.Entities.Remember.Remember>, IRepository<Domain.Schedule.Entities.Remember.Remember, RememberId, RememberIdParams>
{
    public RememberRepository(ApplicationContext db) : base(db)
    {
    }

    private static string GetSequenceName(ComplexScheduleId schedule, ComplexCardId card)
    {
        return $"remember_" +
               $"schedule_{schedule.ParentUserId}_{schedule.Id}_" +
               $"card_{card.UserId}_{card.CollectionId}_{card.Id}";
    }

    public Result<RememberId> GetUniqueId(RememberIdParams param)
    {
        var (schedule, card) = param;
        var sequenceName = GetSequenceName(
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