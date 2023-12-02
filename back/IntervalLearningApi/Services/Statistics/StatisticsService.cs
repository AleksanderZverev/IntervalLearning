using DB;
using DB.Models.ValueObjects;
using Domain.User.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace IntervalLearningApi.Services.Statistics;

public record CalendarLearningStatistic(
    int LearnedCards,
    Dictionary<DateTime, int> DateToLearnedCards,
    Dictionary<DateTime, int> DateToRepeatedCards,
    Dictionary<DateTime, int> DateQueueCards,
    Dictionary<DateTime, int> DateToRecommendationToLearn);

public class StatisticsService
{
    private readonly ILogger<StatisticsService> logger;
    private readonly IHostEnvironment env;
    private readonly ApplicationContext db;

    public StatisticsService(ILogger<StatisticsService> logger,
        IHostEnvironment env,
        ApplicationContext db)
    {
        this.logger = logger;
        this.env = env;
        this.db = db;
    }

    public async Task<CalendarLearningStatistic?> GetLearningStatistic(
        UserId userId,
        UserId scheduleUserId,
        ScheduleId scheduleId,
        DateTime from,
        DateTime to, 
        TimeSpan timezoneOffset)
    {
        var rangeRemembers = await db.Remembers
            .Where(r => r.ParentUserId == userId 
                        && r.ParentRepeatsScheduleUserId == scheduleUserId 
                        && r.ParentRepeatsScheduleId == scheduleId 
                        && r.RepeatedDate >= from 
                        && r.RepeatedDate <= to)
            .ToListAsync();

        var cardToRemembers = rangeRemembers.GroupBy(r => (r.ParentCollectionId, r.ParentCardId));

        var totalLearnedCards = 0;
        var dateToLearnedCards = new Dictionary<DateTime, int>();
        var dateToRepeatedCards = new Dictionary<DateTime, int>();

        foreach (var cardPair in cardToRemembers)
        {
            var card = await db.Cards
                .Include(c => c.Remembers)
                .SingleAsync(c =>
                    c.ParentUserId == userId
                    && c.ParentCollectionId == cardPair.Key.ParentCollectionId
                    && c.Id == cardPair.Key.ParentCardId);

            var learnedDate = card.GetLearnedDate();

            if (learnedDate >= from && learnedDate <= to)
            {
                totalLearnedCards++;
            }
            
            AddOrIncrementDate(dateToLearnedCards, learnedDate, timezoneOffset);

            foreach (var repeatedRemember in card.GetRepeatingRemembers().DistinctBy(r => r.RepeatedDate.Date))
                AddOrIncrementDate(dateToRepeatedCards, repeatedRemember.RepeatedDate, timezoneOffset);
        }

        var dateToQueueCount = await GetDateToQueueCount(userId, scheduleUserId, scheduleId, from, to, timezoneOffset);

        var dateToRecommendationToLearn = await GetDateToRecommendationToLearn(
            userId,
            scheduleUserId, 
            scheduleId, 
            from,
            to,
            timezoneOffset,
            dateToQueueCount);

        return new CalendarLearningStatistic(
            totalLearnedCards,
            dateToLearnedCards,
            dateToRepeatedCards,
            dateToQueueCount,
            dateToRecommendationToLearn);
    }

    static DateTime GetUserLocalDate(DateTime dateTime, TimeSpan offset)
    {
        return (dateTime + offset).Date;
    }
    
    static void AddOrIncrementDate(Dictionary<DateTime, int> dict, DateTime value, TimeSpan offset)
    {
        var userLocalDate = GetUserLocalDate(value, offset);
        dict.TryAdd(userLocalDate, 0);
        dict[userLocalDate]++;
    }

    private async Task<Dictionary<DateTime, int>> GetDateToRecommendationToLearn(
        UserId userId,
        UserId scheduleUserId,
        long scheduleId,
        DateTime from,
        DateTime to,
        TimeSpan timezoneOffset,
        Dictionary<DateTime,int> dateToRepetitionsCount)
    {
        var now = DateTime.UtcNow;

        if (now < from || now > to)
        {
            return new Dictionary<DateTime, int>();
        }

        var schedule = await db.RepeatsSchedules
            .Include(s => s.Phases)
            .SingleAsync(s => s.Id == scheduleId && s.ParentUserId == scheduleUserId);
        
        var orderedPhasesWithoutRepetitions = schedule.Phases
            .Where(p => p.GetDurationToNextPhase() > TimeSpan.FromHours(1) && p.GetDurationToNextPhase().TotalDays <= 40)
            .OrderBy(p => p.SecondsFromLastPhase)
            .ToList();

        var currentDate = from;
        to = to.Date;

        const int maxCardsToRepeat = 115;
        const int maxCardCanBeLearnedForDay = 30;
        
        var result = new Dictionary<DateTime, int>();

        while (currentDate.Date <= to.Date)
        {
            var cardsToLearn = maxCardCanBeLearnedForDay;

            var phaseDate = currentDate;
            foreach (var phase in orderedPhasesWithoutRepetitions)
            {
                phaseDate = phase.GetNextDate(phaseDate);
                
                var cardsToRepeat = dateToRepetitionsCount.TryGetValue(GetUserLocalDate(phaseDate, timezoneOffset), out var repetitionsCount) 
                    ? repetitionsCount 
                    : 0;

                cardsToLearn = Math.Min(cardsToLearn, Math.Max(maxCardsToRepeat - cardsToRepeat, 0));
            }

            result.Add(GetUserLocalDate(currentDate, timezoneOffset), cardsToLearn);
            currentDate = currentDate.AddDays(1);
        }

        return result;
    }
    
    private async Task<Dictionary<DateTime, int>> GetDateToQueueCount(
        UserId userId,
        UserId scheduleUserId,
        ScheduleId scheduleId,
        DateTime from,
        DateTime to,
        TimeSpan timezoneOffset)
    {
        //GetByRange
        var repetitions = db.Queue
            .Where(q =>
                //filter by schedule
                q.ParentUserId == userId
                && q.ParentRepeatsScheduleUserId == scheduleUserId
                && q.ParentRepeatsScheduleId == scheduleId
                //filter by date
                && q.Date >= from && q.Date <= to)
            .ToList();

        return repetitions
            .GroupBy(d => GetUserLocalDate(d.Date, timezoneOffset))
            .ToDictionary(d => d.Key, g => g.Count());
    }

    public async Task<LearningStatistic> GetStatistic(UserId userId, DateTime dateTime)
    {
        var date = dateTime.Date;

        var dateRemembers = await db.Remembers
            .Where(r => r.ParentUserId == userId && r.RepeatedDate.Date == date)
            .Include(r => r.ParentCard)
            .ThenInclude(c => c.Remembers)
            .ToListAsync();

        var cards = dateRemembers
            .Select(r => r.ParentCard)
            .DistinctBy(c => c.ParentCollectionId + "-" + c.Id)
            .ToList();

        var repeatedCards = 0;
        var learnedCards = 0;
        
        foreach (var card in cards)
        {
            var startedDate = card.GetLearnedDate();
            if (startedDate.Date == date)
            {
                learnedCards++;
                continue;
            }

            repeatedCards++;
        }

        return new LearningStatistic(
            RepeatedCards: repeatedCards,
            LearnedCards: learnedCards
        );
    }
    
    public record LearningStatistic(
        int RepeatedCards,
        int LearnedCards
    );
}