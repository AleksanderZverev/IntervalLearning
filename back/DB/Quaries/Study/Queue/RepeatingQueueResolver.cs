using Application.Common.Interfaces.Domain.Study.Queue;
using DB.Models.ValueObjects;
using Domain.Card.ValueObjects;
using Domain.Collection.ValueObjects;
using Domain.Queue;
using Domain.User.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace DB.Resolvers.Study.Queue;

public class RepeatingQueueResolver : IRepeatingQueueResolver
{
    private readonly ApplicationContext db;

    public RepeatingQueueResolver(ApplicationContext db)
    {
        this.db = db;
    }

    public Task<List<CardRepeatQueue>> GetAll(UserId userId)
    {
        return db.Queue
            .Where(q => q.ParentUserId == userId)
            .Include(q => q.ParentRepeatsSchedule)
            .ThenInclude(q => q.Phases)
            .AsSplitQuery()
            .ToListAsync();
    }

    public Task<List<CardRepeatQueue>> GetByDate(
        UserId userId,
        CollectionId collectionId,
        UserId scheduleUserId,
        ScheduleId scheduleId,
        short phaseIndex,
        DateTime dateTime)
    {
        return db.Queue
            .Where(c => c.ParentUserId == userId
                        && c.ParentCollectionId == collectionId
                        && c.ParentRepeatsScheduleUserId == scheduleUserId
                        && c.ParentRepeatsScheduleId == scheduleId
                        && c.PhaseIndex == phaseIndex
                        && c.Date.Date == dateTime.Date)
            .ToListAsync();
    }
    
    public Task<List<CardRepeatQueue>> GetByRange(
        UserId userId,
        UserId scheduleUserId,
        ScheduleId scheduleId,
        DateTime from,
        DateTime to)
    {
        return db.Queue
            .Where(q =>
                //filter by schedule
                q.ParentUserId == userId
                && q.ParentRepeatsScheduleUserId == scheduleUserId
                && q.ParentRepeatsScheduleId == scheduleId
                //filter by date
                && q.Date >= from && q.Date <= to)
            .ToListAsync();
    }
    
    public Task<List<CardRepeatQueue>> GetForCards(
        UserId userId,
        CollectionId collectionId,
        UserId scheduleUserId,
        ScheduleId scheduleId,
        short phaseIndex,
        List<CardId> cardIds)
    {
        return db.Queue
            .Where(q => q.ParentUserId == userId
                        && q.ParentCollectionId == collectionId
                        && q.ParentRepeatsScheduleUserId == scheduleUserId
                        && q.ParentRepeatsScheduleId == scheduleId
                        && q.PhaseIndex == phaseIndex
                        && cardIds.Contains(q.ParentCardId))
            .ToListAsync();
    }
}