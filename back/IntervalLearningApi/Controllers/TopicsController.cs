using AutoMapper;
using IntervalLearningApi.Extensions;
using IntervalLearningApi.Models.Pagination;
using IntervalLearningApi.Models.Requests;
using IntervalLearningApi.Models.Topics;
using IntervalLearningApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace IntervalLearningApi.Controllers;

[Route("api/courses/{courseId:long}/topics")]
[Authorize]
[ApiController]
public class TopicsController : ControllerBase
{
    private readonly IMapper mapper;
    private readonly TopicsService topicsService;

    public TopicsController(IMapper mapper, TopicsService topicsService)
    {
        this.mapper = mapper;
        this.topicsService = topicsService;
    }

    [HttpPost]
    public async Task<ActionResult<Topic>> Create(long courseId, CreateTopicRequest request)
    {
        var (topicEntity, error) = await topicsService.Create(
            HttpContext.GetUserId(),
            courseId,
            new CreateTopicParameters(request.Name, request.Theory));

        return topicEntity != null
            ? mapper.Map<Topic>(topicEntity)
            : BadRequest(error);
    }

    [HttpPost("{topicId:long}")]
    public async Task<ActionResult<Topic>> Patch(long courseId, long topicId, [FromBody] PatchTopicRequest request)
    {
        var (topicEntity, error) = await topicsService.Patch(
            HttpContext.GetUserId(),
            courseId,
            topicId,
            new PatchTopicParameters(request.Name, request.Theory));

        return topicEntity != null
            ? mapper.Map<Topic>(topicEntity)
            : BadRequest(error);
    }

    [HttpGet("{topicId:long}")]
    public async Task<ActionResult<Topic>> Get(long courseId, long topicId)
    {
        var course = await topicsService.Get(courseId, topicId);

        return course != null
            ? mapper.Map<Topic>(course)
            : NotFound();
    }

    [HttpGet]
    public async Task<ActionResult<SearchResult<Topic>>> Search(
        long courseId,
        [FromQuery] string? name,
        [FromQuery] int page = 1,
        [FromQuery] int count = 10)
    {
        var (topicEntities, totalCount) = await topicsService.Search(courseId, name?.ToLower(), page, count);

        return new SearchResult<Topic>
        {
            FoundItems = topicEntities.Select(mapper.Map<Topic>).ToList(),
            TotalCount = totalCount
        };
    }

    [HttpDelete("{topicId:long}")]
    public async Task<ActionResult<Topic>> Delete(long courseId, long topicId)
    {
        var (topicEntity, error) = await topicsService.Delete(
            HttpContext.GetUserId(),
            courseId,
            topicId);

        return topicEntity != null
            ? mapper.Map<Topic>(topicEntity) 
            : BadRequest(error);
    }
}