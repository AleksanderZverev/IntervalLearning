using DB.Models;
using DB.Models.Dictionary;
using DB.Models.Store;
using Domain.Language;
using Domain.User.ValueObjects;
using IntervalLearningApi.Constants;
using IntervalLearningApi.Extensions;
using IntervalLearningApi.Models.ByUser;
using IntervalLearningApi.Models.Dictionary;
using IntervalLearningApi.Services;
using MapsterMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IntervalLearningApi.Controllers
{
    [Route(ApiRoutes.Collections.BasePath)]
    [Authorize]
    [ApiController]
    public class CollectionsController : ControllerBase
    {
        private readonly IMapper mapper;
        private readonly CollectionService collectionService;

        public CollectionsController(
            IMapper mapper,
            CollectionService collectionService)
        {
            this.mapper = mapper;
            this.collectionService = collectionService;
        }

        [HttpPost(ApiRoutes.Collections.Create)]
        public ActionResult<Collection> CreateCollection([FromBody]CreateCollectionItem item)
        {
            var userId = HttpContext.GetUserId();

            if (userId.IsFailed)
                return BadRequest();

            var (collection, error) = collectionService.CreateOrEdit(
                new CollectionService.CreateOrPatchCollection(
                    userId.Value,
                    item.Title,
                    item.IsDefaultBackSide,
                    item.ThemeId
                ),
                item.CollectionId);

            return collection != null
                ? mapper.Map<Collection>(collection)
                : BadRequest(error);
        }
        
        [HttpGet(ApiRoutes.Collections.SearchPublic)]
        public async Task<ActionResult<List<StoreCollection>>> SearchPublicCollection(short themeId, string? searchName = null, int page = 1, int count = 10)
        {
            var userId = HttpContext.GetUserId();

            if (userId.IsFailed)
                return BadRequest();

            var collections = await collectionService.SearchPublicCollections(userId.Value, themeId, searchName ?? "", page, count);
            return mapper.Map<List<StoreCollection>>(collections.Select((t) => (t.collection, t.subscriber)));
        }

        [HttpGet(ApiRoutes.Collections.SearchPrivate)]
        public async Task<ActionResult<List<Collection>>> SearchCollection(short themeId, string? searchName = null, int page = 1, int count = 10)
        {
            var userId = HttpContext.GetUserId();

            if (userId.IsFailed)
                return BadRequest();

            var collections = await collectionService.SearchCollections(userId.Value, themeId, searchName ?? "", page, count);
            return mapper.Map<List<Collection>>(collections);
        }

        [AllowAnonymous]
        [HttpGet(ApiRoutes.Collections.GetPublicCollection)]
        public async Task<ActionResult<Collection>> GetPublicCollection(long userId, short collectionId)
        {
            var collection = await collectionService.FindPublicCollection(userId, collectionId);
            return collection == null 
                ? NotFound() 
                : mapper.Map<Collection>(collection);
        }

        [HttpGet(ApiRoutes.Collections.GetAll)]
        public async Task<ActionResult<List<Collection>>> GetAll()
        {
            var userId = HttpContext.GetUserId();

            if (userId.IsFailed)
                return BadRequest();

            var collections = await collectionService.GetAllByUserId(userId.Value);
            return mapper.Map<List<Collection>>(collections);
        }

        [HttpGet(ApiRoutes.Collections.GetRandomWords)]
        public async Task<ActionResult<GetRandomWordResponse>> GetRandomWords([FromQuery]short collectionId)
        {
            var userId = HttpContext.GetUserId();

            if (userId.IsFailed)
                return BadRequest();

            var (words, language, error) = await collectionService.GetRandomWords(userId.Value, collectionId);

            return words == null || language == null
                ? BadRequest(error)
                : new GetRandomWordResponse(
                    mapper.Map<List<WordDto>>(words),
                    mapper.Map<LanguageDto>(language));
        }

        [HttpGet(ApiRoutes.Collections.GetRepeatCollections)]
        public async Task<ActionResult<RepeatingCollectionResponse>> GetRepeatCollections()
        {
            var userId = HttpContext.GetUserId();

            if (userId.IsFailed)
                return BadRequest();

            var dateToRepeatingCollections = await collectionService.GetRepeatCollections(userId.Value);

            return new RepeatingCollectionResponse(dateToRepeatingCollections
                .ToDictionary(
                    p => p.Key,
                    p => p.Value.Select(c => mapper.Map<RepeatingPhaseDto>(c)).ToList()));
        }

        [HttpGet(ApiRoutes.Collections.GetNotFinished)]
        public async Task<ActionResult<GetNotFinishedResponse>> GetNotFinished(
            long scheduleUserId,
            short scheduleId,
            int page = 1, 
            int count = 30)
        {
            var userId = HttpContext.GetUserId();

            if (userId.IsFailed)
                return BadRequest();

            var (totalCollections, canStartCollections) = await collectionService.GetCanStart(userId.Value, UserId.Create(scheduleUserId).Value, scheduleId, page, count);
            return new GetNotFinishedResponse(
                totalCollections,
                mapper.Map<List<Collection>>(canStartCollections));
        }

        [HttpGet(ApiRoutes.Collections.GetCollection)]
        public async Task<ActionResult<Collection>> Get(short collectionId)
        {
            var userId = HttpContext.GetUserId();

            if (userId.IsFailed)
                return BadRequest();

            var collection = await collectionService.Find(userId.Value, collectionId).ConfigureAwait(false);
            return collection != null 
                ? mapper.Map<Collection>(collection)
                : NotFound();
        }

        [HttpPost(ApiRoutes.Collections.MakePublic)]
        public async Task<ActionResult<Collection>> MakePublic(short collectionId)
        {
            var userId = HttpContext.GetUserId();

            if (userId.IsFailed)
                return BadRequest();

            var (collection, error) = await collectionService.MakePublic(userId.Value, collectionId).ConfigureAwait(false);
            return collection != null 
                ? mapper.Map<Collection>(collection) 
                : BadRequest(error);
        }

        [HttpPost(ApiRoutes.Collections.AddCardsToMyCollection)]
        public async Task<ActionResult<Collection>> AddCardsToMyCollection(
            long collectionUserId,
            short collectionId,
            [FromBody] AddCollectionsRequest request)
        {
            var userId = HttpContext.GetUserId();

            if (userId.IsFailed)
                return BadRequest();

            var (collection, error) = await collectionService.AddCardsToMyCollection(
                UserId.Create(collectionUserId).Value,
                collectionId,
                userId.Value,
                request.MyCollectionId,
                request.NewCollectionName,
                request.CheckUnique);
            
            return collection != null 
                ? mapper.Map<Collection>(collection)
                : BadRequest(error);
        }
    }

    public class GetRandomWordResponse
    {
        public List<WordDto> Words { get; }

        public LanguageDto Language { get; }

        public GetRandomWordResponse(
            List<WordDto> words, 
            LanguageDto language)
        {
            Words = words;
            Language = language;
        }
    }

    public class AddCollectionsRequest
    {
        public bool CheckUnique { get; set; }
        public short? MyCollectionId { get; set; }
        public string? NewCollectionName { get; set; }
    }
}
