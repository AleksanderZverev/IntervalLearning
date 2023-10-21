using IntervalLearningApi.Constants;
using IntervalLearningApi.Extensions;
using IntervalLearningApi.Models.ByUser;
using IntervalLearningApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace IntervalLearningApi.Controllers;

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
        var statistic = await statisticsService.GetStatistic(HttpContext.GetUserId(), dateTime);

        return new LearningStatisticModel(
            RepeatedCards: statistic.RepeatedCards,
            LearnedCards: statistic.LearnedCards);
    }
    
    [HttpGet(ApiRoutes.Statistics.Get_DetailedCalendarStatistic)]
    public async Task<ActionResult<CalendarLearningStatisticModel>> GetStatisticWithRecommendation(
        long scheduleUserId,
        long scheduleId,
        DateTime from,
        DateTime to,
        int timezoneOffsetInMinutes)
    {
        var statistic = await statisticsService.GetLearningStatistic(
            HttpContext.GetUserId(),
            scheduleUserId,
            scheduleId,
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