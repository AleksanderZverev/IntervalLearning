using Domain.Schedule.ValueObjects;
using Domain.Theme.ValueObjects;
using Domain.User.ValueObjects;
using GlobalTools.Extensions;
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
    public async Task<ActionResult<LearningStatisticModel>> GetLearningStatistic(
        [FromQuery] long scheduleUserId,
        [FromQuery] short scheduleId,
        [FromQuery] short themeId,
        [FromQuery(Name = "date")] DateTimeOffset statisticDate,
        [FromQuery] DateTimeOffset userCurrentDateTime)
    {
        if (scheduleUserId == 0 || scheduleId == 0 || themeId == 0)
            return BadRequest();

        var argsResult = (
            HttpContext.GetUserId(),
            UserId.Create(scheduleUserId),
            ScheduleId.Create(scheduleId),
            ThemeId.Create(themeId));

        if (argsResult.HasAnyError())
            return BadRequest();

        var (userIdResult, scheduleUserIdResult, scheduleIdResult, themeIdResult) = argsResult;

        var statistic = await statisticsService.GetStatistic(
            userIdResult.Value,
            scheduleUserIdResult.Value,
            scheduleIdResult.Value,
            themeIdResult.Value,
            statisticDate,
            userCurrentDateTime);

        return new LearningStatisticModel(
            TotalRepeatingCards: statistic.TotalRepeatingCards,
            PhaseIdToStatistic: statistic.PhaseStatistics
                .AsEnumerable()
                .ToDictionary(p => p.Key.Value.ToString(), p => new PhaseStatisticDto()
                {
                    PhaseId = p.Key.Value.ToString(),
                    TotalRepeatingCards = p.Value.TotalRepeatingCards,
                    LateCards = p.Value.LateCards,
                    FutureCards = p.Value.FutureCards,
                    TodayCards = p.Value.TodayCards,
                }),
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