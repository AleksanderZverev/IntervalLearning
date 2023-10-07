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
    private readonly CardsService cardsService;

    public StatisticsController(CardsService cardsService)
    {
        this.cardsService = cardsService;
    }
    
    [HttpGet(ApiRoutes.Statistics.Get_LearningStatistic)]
    public async Task<ActionResult<LearningStatisticModel>> GetLearningStatistic([FromQuery(Name = "date")] DateTime dateTime)
    {
        var statistic = await cardsService.GetStatistic(HttpContext.GetUserId(), dateTime);

        return new LearningStatisticModel(
            RepeatedCards: statistic.RepeatedCards,
            LearnedCards: statistic.LearnedCards);
    }
}