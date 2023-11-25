using Application.Common.Interfaces.Domain.Study.Remember;
using DB.Models.ValueObjects;
using Domain.Card.ValueObjects;
using Domain.Collection.ValueObjects;
using Domain.User.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace DB.Resolvers.Study.Remember;

public class RememberQueryQueryResolver : IRememberQueryResolver
{
    private readonly ApplicationContext db;

    public RememberQueryQueryResolver(ApplicationContext db)
    {
        this.db = db;
    }
    
    public Task<List<Domain.Schedule.Entities.Remember.Remember>> GetRangeForCollection(
        UserId userId,
        CollectionId collectionId,
        UserId scheduleUserId,
        ScheduleId scheduleId)
    {
        return db.Remembers.Where(r => r.ParentUserId == userId
                                       && r.ParentCollectionId == collectionId
                                       && r.ParentRepeatsScheduleUserId == scheduleUserId
                                       && r.ParentRepeatsScheduleId == scheduleId)
            .ToListAsync();
    }

    public Task<List<Domain.Schedule.Entities.Remember.Remember>> GetRangeForCards(
        UserId userId,
        CollectionId collectionId,
        UserId scheduleUserId,
        ScheduleId scheduleId,
        List<CardId> cardsIds)
    {
        return db.Remembers.Where(r => r.ParentUserId == userId
                                       && r.ParentCollectionId == collectionId
                                       && r.ParentRepeatsScheduleUserId == scheduleUserId
                                       && r.ParentRepeatsScheduleId == scheduleId
                                       && cardsIds.Contains(r.ParentCardId))
            .ToListAsync();
    }
}