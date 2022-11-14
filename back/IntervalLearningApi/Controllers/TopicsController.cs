using IntervalLearningApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace IntervalLearningApi.Controllers;

[Route("api/topics")]
[Authorize]
[ApiController]
public class TopicsController : ControllerBase
{
    private readonly TopicsService topicsService;

    public TopicsController(TopicsService topicsService)
    {
        this.topicsService = topicsService;
    }
}