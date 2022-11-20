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

    public TopicsController(TopicsService topicsService)
    {
        this.topicsService = topicsService;
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
            ? ToTopic(topicEntity)
            : BadRequest(error);
    }

    [HttpGet("all/{parentCourseId:long}")]
    public async Task<ActionResult<List<Topic>>> GetAll(long parentCourseId, [FromQuery] int page, [FromQuery] int count)
    {
        var topicEntities = await topicsService.GetAll(parentCourseId, page, count);
        return topicEntities.Select(ToTopic).ToList();
    }

    [HttpGet("{id:long}")]
    public async Task<ActionResult<Topic>> Get(long id)
    {
        var topicEntity = await topicsService.Get(id);
        return topicEntity != null
            ? ToTopic(topicEntity)
            : NotFound();
    }

    private static Topic ToTopic(TopicEntity course)
    {
        return new Topic
        {
            Id = course.Id,
            ParentCourseId = course.ParentCourseId,
            Name = course.Name,
            CourseCollections = course.CourseCollections,
            Theory = course.Theory
        };
    }
}

public class CreateTopicRequest
{
    public string Name { get; set; }
    public Guid ParentCourseId { get; set; }
    public string Theory { get; set; }
}
