using Domain.Card.ValueObjects;
using Domain.Collection.ValueObjects;
using Domain.Queue;
using Domain.Schedule.ValueObjects;
using Domain.User.ValueObjects;
using DomainServices.DB.Queries.Study.Queue;
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

    public Task<List<CardRepeatQueue>> GetAllBySchedule(UserId userId, ComplexScheduleId scheduleId)
    {
        return db.Queue
            .Where(q => q.ParentUserId == userId
                        && q.ParentRepeatsScheduleUserId == scheduleId.ParentUserId
                        && q.ParentRepeatsScheduleId == scheduleId.Id)
            .ToListAsync();
    }

    public async Task<(List<CardRepeatQueue> QueuedCardInfos, int TotalCards)> GetAllTillDate(
        int page,
        int cout,
        UserId userId,
        CollectionId collectionId,
        UserId scheduleUserId,
        ScheduleId scheduleId,
        DateTime date)
    {
        var skip = (page - 1) * cout;
        var take = cout;
        var query = db.Queue
            .Where(c => c.ParentUserId == userId
                        && c.ParentCollectionId == collectionId
                        && c.ParentRepeatsScheduleUserId == scheduleUserId
                        && c.ParentRepeatsScheduleId == scheduleId
                        && c.Date.Date <= date.Date);
        
        var cards = await query.Skip(skip).Take(take).ToListAsync();
        var cardsCount = await query.CountAsync();
        return (cards, cardsCount);
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
        List<CardId> cardIds)
    {
        return db.Queue
            .Where(q => q.ParentUserId == userId
                        && q.ParentCollectionId == collectionId
                        && q.ParentRepeatsScheduleUserId == scheduleUserId
                        && q.ParentRepeatsScheduleId == scheduleId
                        && cardIds.Contains(q.ParentCardId))
            .ToListAsync();
    }

    public Task<List<CardRepeatQueue>> GetAllForCard(
        UserId userId,
        CollectionId collectionId,
        CardId cardId,
        UserId scheduleUserId,
        ScheduleId scheduleId)
    {
        return db.Queue
            .Where(q => q.ParentUserId == userId
                        && q.ParentCollectionId == collectionId
                        && q.ParentCardId == cardId
                        && q.ParentRepeatsScheduleUserId == scheduleUserId
                        && q.ParentRepeatsScheduleId == scheduleId)
            .ToListAsync();
    }
}