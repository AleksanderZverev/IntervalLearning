using Domain.Schedule.ValueObjects;
using Domain.User.ValueObjects;
using IntervalLearningApi.Constants;
using IntervalLearningApi.Controllers.Study.Statistics.DTOs;
using IntervalLearningApi.Extensions;
using IntervalLearningApi.Services.Statistics;
using Microsoft.AspNetCore.Mvc;

namespace IntervalLearningApi.Controllers.Study.Statistics;

[Route(ApiRoutes.Statistics.BasePath)]
[Authorize]
[ApiController]
public class StatisticsController : ControllerBase
{
    private readonly StatisticsService statisticsService;

    public StatisticsController(StatisticsService statisticsService)
    {
        this.statisticsService = statisticsService;
    }
    
    [HttpGet(ApiRoutes.Statistics.Get_LearningStatistic)]
    public async Task<ActionResult<LearningStatisticModel>> GetLearningStatistic([FromQuery(Name = "date")] DateTime dateTime)
    {
        var userId = HttpContext.GetUserId();

        if (userId.IsFailed)
            return BadRequest();
        
        var statistic = await statisticsService.GetStatistic(userId.Value, dateTime);

        return new LearningStatisticModel(
            RepeatedCards: statistic.RepeatedCards,
            LearnedCards: statistic.LearnedCards);
    }
    
    [HttpGet(ApiRoutes.Statistics.Get_DetailedCalendarStatistic)]
    public async Task<ActionResult<CalendarLearningStatisticModel>> GetStatisticWithRecommendation(
        long scheduleUserId,
        short scheduleId,
        DateTime from,
        DateTime to,
        int timezoneOffsetInMinutes)
    {
        var userId = HttpContext.GetUserId();

        if (userId.IsFailed)
            return BadRequest();

        var statistic = await statisticsService.GetLearningStatistic(
            userId.Value,
            UserId.Create(scheduleUserId).Value,
            ScheduleId.Create(scheduleId).Value,
            from,
            to,
            TimeSpan.FromMinutes(timezoneOffsetInMinutes));

        if (statistic == null)
            return BadRequest();

        return new CalendarLearningStatisticModel(
            LearnedCards: statistic.LearnedCards,
            DateToLearnedCards: statistic.DateToLearnedCards,
            DateQueueCards: statistic.DateQueueCards,
            DateToRepeatedCards: statistic.DateToRepeatedCards,
            DateToRecommendationToLearn: statistic.DateToRecommendationToLearn);
    }
}