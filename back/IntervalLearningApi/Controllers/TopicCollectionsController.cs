using AutoMapper;
using IntervalLearningApi.Extensions;
using IntervalLearningApi.Models.Requests;
using IntervalLearningApi.Models.Topics.TopicCollections;
using IntervalLearningApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace IntervalLearningApi.Controllers;

[Route("api/courses/{courseId:long}/topics/{topicId:long}/topic-collections")]
[Authorize]
[ApiController]
public class TopicCollectionsController : ControllerBase
{
    private readonly IMapper mapper;
    private readonly TopicCollectionsService topicCollectionsService;

    public TopicCollectionsController(IMapper mapper, TopicCollectionsService topicCollectionsService)
    {
        this.mapper = mapper;
        this.topicCollectionsService = topicCollectionsService;
    }

    [HttpPost]
    public async Task<ActionResult<TopicCollection>> Create(
        long courseId,
        long topicId,
        CreateTopicCollectionRequest request)
    {
        var (topicCollectionEntity, error) = await topicCollectionsService.Create(
            HttpContext.GetUserId(),
            courseId,
            topicId,
            new CreateTopicCollectionParameters(request.Name));

        return topicCollectionEntity != null
            ? mapper.Map<TopicCollection>(topicCollectionEntity)
            : BadRequest(error);
    }

    [HttpPatch("{topicCollectionId:long}")]
    public async Task<ActionResult<TopicCollection>> Patch(
        long courseId,
        long topicId,
        long topicCollectionId,
        PatchTopicCollectionRequest request)
    {
        var (topicCollectionEntity, error) = await topicCollectionsService.Patch(
            HttpContext.GetUserId(),
            courseId,
            topicId,
            topicCollectionId,
            new PatchTopicCollectionParameters(request.Name));

        return topicCollectionEntity != null
            ? mapper.Map<TopicCollection>(topicCollectionEntity)
            : BadRequest(error);
    }
    
    [HttpGet]
    public async Task<ActionResult<List<TopicCollection>>> Search(
        long courseId,
        long topicId,
        [FromQuery] string? name,
        [FromQuery] int page,
        [FromQuery] int count)
    {
        var topicEntities = await topicCollectionsService.SearchByName(courseId, topicId, name?.ToLower(), page, count);

        return topicEntities.Select(mapper.Map<TopicCollection>).ToList();
    }

    [HttpDelete("{topicCollectionId:long}")]
    public async Task<ActionResult<TopicCollection>> Delete(long courseId, long topicId, long topicCollectionId)
    {
        var (topicEntity, error) = await topicCollectionsService.Delete(
            HttpContext.GetUserId(),
            courseId,
            topicId,
            topicCollectionId);

        return topicEntity != null
            ? mapper.Map<TopicCollection>(topicEntity) 
            : BadRequest(error);
    }
}