using AutoMapper;
using IntervalLearningApi.Extensions;
using IntervalLearningApi.Models;
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
        var (topicCollectionEntity, error) = await topicCollectionsService.CreateTopicCollection(
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
        var (topicCollectionEntity, error) = await topicCollectionsService.PatchTopicCollection(
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
        var topicEntities = await topicCollectionsService.SearchTopicCollections(courseId, topicId, name?.ToLower(), page, count);

        return topicEntities.Select(mapper.Map<TopicCollection>).ToList();
    }

    [HttpDelete("{topicCollectionId:long}")]
    public async Task<ActionResult<TopicCollection>> Delete(long courseId, long topicId, long topicCollectionId)
    {
        var (topicEntity, error) = await topicCollectionsService.DeleteTopicCollection(
            HttpContext.GetUserId(),
            courseId,
            topicId,
            topicCollectionId);

        return topicEntity != null
            ? mapper.Map<TopicCollection>(topicEntity) 
            : BadRequest(error);
    }
    
    [HttpPost("{topicCollectionId:long}/cards")]
    public async Task<ActionResult<TopicCard>> CreateCard(
        long courseId,
        long topicId,
        long topicCollectionId,
        CreateTopicCardRequest request)
    {
        var (topicCollectionEntity, error) = await topicCollectionsService.CreateTopicCard(
            HttpContext.GetUserId(),
            courseId,
            topicId,
            topicCollectionId,
            mapper.Map<CreateTopicCardParameters>(request));

        return topicCollectionEntity != null
            ? mapper.Map<TopicCard>(topicCollectionEntity)
            : BadRequest(error);
    }
    
    [HttpPatch("{topicCollectionId:long}/cards/{topicCardId:long}")]
    public async Task<ActionResult<TopicCard>> PatchCard(
        long courseId,
        long topicId,
        long topicCollectionId,
        long topicCardId,
        PatchTopicCardRequest request)
    {
        var (topicCollectionEntity, error) = await topicCollectionsService.PatchTopicCard(
            HttpContext.GetUserId(),
            courseId,
            topicId,
            topicCollectionId,
            topicCardId,
            mapper.Map<PatchTopicCardParameters>(request));

        return topicCollectionEntity != null
            ? mapper.Map<TopicCard>(topicCollectionEntity)
            : BadRequest(error);
    }
    
    [HttpGet("{topicCollectionId:long}/cards/")]
    public async Task<ActionResult<List<TopicCard>>> SearchCard(
        long courseId,
        long topicId,
        long topicCollectionId,
        [FromQuery] string value,
        [FromQuery] SearchFieldType fieldType,
        [FromQuery] int page = 1,
        [FromQuery] int count = 10)
    {
        var topicCardEntities = await topicCollectionsService.SearchTopicCards(
            courseId,
            topicId,
            topicCollectionId,
            value,
            fieldType,
            page,
            count);

        return topicCardEntities.Select(mapper.Map<TopicCard>).ToList();
    }

    [HttpDelete("{topicCollectionId:long}/cards/{topicCardId:long}")]
    public async Task<ActionResult<TopicCard>> Delete(
        long courseId,
        long topicId,
        long topicCollectionId,
        long topicCardId)
    {
        var (topicCard, error) = await topicCollectionsService.DeleteTopicCard(
            HttpContext.GetUserId(),
            courseId,
            topicId,
            topicCollectionId,
            topicCardId);

        return topicCard != null
            ? mapper.Map<TopicCard>(topicCard) 
            : BadRequest(error);
    }
}