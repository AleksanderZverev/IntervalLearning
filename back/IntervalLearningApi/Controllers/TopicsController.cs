using AutoMapper;
using DB.Models;
using IntervalLearningApi.Models.Topics;
using IntervalLearningApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace IntervalLearningApi.Controllers;

[Route("api/topics")]
[Authorize]
[ApiController]
public class TopicsController : ControllerBase
{
    private readonly TopicsService topicsService;
    private readonly IMapper mapper;

    public TopicsController(TopicsService topicsService, IMapper mapper)
    {
        this.topicsService = topicsService;
        this.mapper = mapper;
    }

    [HttpPost]
    public ActionResult<Topic> Create(CreateTopicRequest request)
    {
        var (topicEntity, error) = topicsService.CreateOrEdit(
            new CreateOrPatchTopic
            {
                ParentCourseId = request.ParentCourseId,
                Name = request.Name,
                Theory = request.Theory
            },
            null);

        return topicEntity != null
            ? mapper.Map<Topic>(topicEntity)
            : BadRequest(error);
    }

    [HttpPost("{parentCourseId:long}")]
    public ActionResult<Topic> Patch(long parentCourseId, [FromBody] PatchTopicRequest request)
    {
        var (topicEntity, error) = topicsService.CreateOrEdit(
            new CreateOrPatchTopic
            {
                ParentCourseId = parentCourseId,
                Name = request.Name,
                Theory = request.Theory
            },
            parentCourseId);

        return topicEntity != null
            ? mapper.Map<Topic>(topicEntity)
            : BadRequest(error);
    }

    [HttpGet("all/{parentCourseId:long}")]
    public async Task<ActionResult<List<Topic>>> GetAll(long parentCourseId, [FromQuery] int page, [FromQuery] int count)
    {
        var topicEntities = await topicsService.GetAll(parentCourseId, page, count);
        return topicEntities.Select(mapper.Map<Topic>).ToList();
    }

    [HttpGet("{id:long}")]
    public async Task<ActionResult<Topic>> Get(long id)
    {
        var topicEntity = await topicsService.Get(id);
        return topicEntity != null
            ? mapper.Map<Topic>(topicEntity)
            : NotFound();
    }

    [HttpGet("{parentCourseId:long}")]
    public async Task<ActionResult<List<Topic>>> SearchByName(
        long parentCourseId,
        [FromQuery] string name,
        [FromQuery] int page,
        [FromQuery] int count)
    {
        var topicEntities = await topicsService.SearchByName(parentCourseId, name, page, count);
        return topicEntities.Select(mapper.Map<Topic>).ToList();
    }

    [HttpDelete("{id:long}")]
    public async Task<ActionResult<Topic>> Delete(long id)
    {
        var (topicEntity, error) = await topicsService.Delete(id);
        return topicEntity != null ? mapper.Map<Topic>(topicEntity) : BadRequest(error);
    }
}

public class CreateTopicRequest
{
    public string Name { get; set; }
    public long ParentCourseId { get; set; }
    public string Theory { get; set; }
}

public class PatchTopicRequest
{
    public string Name { get; set; }
    public string Theory { get; set; }
}
