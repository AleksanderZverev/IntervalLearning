using Application.Commands.Collections.CreateCollection;
using Application.Commands.Collections.UpdateCollection;
using DB.Models.ValueObjects;
using Domain.Collection;
using Domain.Collection.ValueObjects;
using Domain.User.ValueObjects;
using FluentResults;
using IntervalLearningApi.Constants;
using IntervalLearningApi.Extensions;
using IntervalLearningApi.Infrastructure.CommandManager;
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
        private readonly CommandManager commandManager;
        private readonly CollectionService collectionService;

        public CollectionsController(
            IMapper mapper,
            CommandManager commandManager,
            CollectionService collectionService)
        {
            this.mapper = mapper;
            this.commandManager = commandManager;
            this.collectionService = collectionService;
        }

        [HttpPost(ApiRoutes.Collections.Create)]
        public async Task<ActionResult<CollectionDto>> CreateCollection([FromBody] CreateCollectionItem item)
        {
            var userId = HttpContext.GetUserId();

            if (userId.IsFailed)
                return BadRequest();

            Result<Collection> collectionResult;

            if (item.CollectionId == null)
            {
                collectionResult = await commandManager
                    .GetCommand<CreateCollectionCommand>()
                    .Handle(new CreateCollectionRequest()
                    {
                        ParentUserId = userId.Value,
                        Title = ThemeTitle.Create(item.Title).Value,
                        ThemeId = ThemeId.Create(item.ThemeId).Value,
                        IsDefaultBackSide = item.IsDefaultBackSide,
                    });
            }
            else
            {
                collectionResult = await commandManager
                    .GetCommand<UpdateCollectionCommand>()
                    .Handle(new UpdateCollectionRequest()
                    {
                        ParentUserId = userId.Value,
                        CollectionId = CollectionId.Create(item.CollectionId.Value).Value,
                        Title = ThemeTitle.Create(item.Title).Value,
                        ThemeId = ThemeId.Create(item.ThemeId).Value,
                        IsDefaultBackSide = item.IsDefaultBackSide,
                    });
            }

            return collectionResult.ToActionResult(collection => mapper.Map<CollectionDto>(collection));
        }

        [HttpGet(ApiRoutes.Collections.SearchPublic)]
        public async Task<ActionResult<List<StoreCollection>>> SearchPublicCollection(short themeId, string? searchName = null, int page = 1, int count = 10)
        {
            var userId = HttpContext.GetUserId();

            if (userId.IsFailed)
                return BadRequest();

            var collections = await collectionService.SearchPublicCollections(userId.Value, ThemeId.Create(themeId).Value, searchName ?? "", page, count);
            return mapper.Map<List<StoreCollection>>(collections.Select((t) => (t.collection, t.subscriber)));
        }

        [HttpGet(ApiRoutes.Collections.SearchPrivate)]
        public async Task<ActionResult<List<CollectionDto>>> SearchCollection(short themeId, string? searchName = null, int page = 1, int count = 10)
        {
            var userId = HttpContext.GetUserId();

            if (userId.IsFailed)
                return BadRequest();

            var collections = await collectionService.SearchCollections(userId.Value, ThemeId.Create(themeId).Value, searchName ?? "", page, count);
            return mapper.Map<List<CollectionDto>>(collections);
        }

        [AllowAnonymous]
        [HttpGet(ApiRoutes.Collections.GetPublicCollection)]
        public async Task<ActionResult<CollectionDto>> GetPublicCollection(long userId, short collectionId)
        {
            var collection = await collectionService.FindPublicCollection(UserId.Create(userId).Value, CollectionId.Create(collectionId).Value);
            return collection == null 
                ? NotFound() 
                : mapper.Map<CollectionDto>(collection);
        }

        [HttpGet(ApiRoutes.Collections.GetAll)]
        public async Task<ActionResult<List<CollectionDto>>> GetAll()
        {
            var userId = HttpContext.GetUserId();

            if (userId.IsFailed)
                return BadRequest();

            var collections = await collectionService.GetAllByUserId(userId.Value);
            return mapper.Map<List<CollectionDto>>(collections);
        }

        [HttpGet(ApiRoutes.Collections.GetRandomWords)]
        public async Task<ActionResult<GetRandomWordResponse>> GetRandomWords([FromQuery]short collectionId)
        {
            var userId = HttpContext.GetUserId();

            if (userId.IsFailed)
                return BadRequest();

            var wordsResult = await collectionService.GetRandomWords(userId.Value, CollectionId.Create(collectionId).Value);

            if (wordsResult.IsFailed)
                return wordsResult.ToErrorActionResult();

            var (words, language) = wordsResult.Value;
            return new GetRandomWordResponse(
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

            var (totalCollections, canStartCollections) = await collectionService.GetCanStart(
                userId.Value,
                UserId.Create(scheduleUserId).Value,
                ScheduleId.Create(scheduleId).Value,
                page,
                count);
            
            return new GetNotFinishedResponse(
                totalCollections,
                mapper.Map<List<CollectionDto>>(canStartCollections));
        }

        [HttpGet(ApiRoutes.Collections.GetCollection)]
        public async Task<ActionResult<CollectionDto>> Get(short collectionId)
        {
            var userId = HttpContext.GetUserId();

            if (userId.IsFailed)
                return BadRequest();

            var collection = await collectionService.Find(userId.Value, CollectionId.Create(collectionId).Value).ConfigureAwait(false);
            return collection != null 
                ? mapper.Map<CollectionDto>(collection)
                : NotFound();
        }

        [HttpPost(ApiRoutes.Collections.MakePublic)]
        public async Task<ActionResult<CollectionDto>> MakePublic(short collectionId)
        {
            var userId = HttpContext.GetUserId();

            if (userId.IsFailed)
                return BadRequest();

            var collectionResult = await collectionService.MakePublic(userId.Value, CollectionId.Create(collectionId).Value).ConfigureAwait(false);
            return collectionResult.ToActionResult(collection => mapper.Map<CollectionDto>(collection));
        }

        [HttpPost(ApiRoutes.Collections.AddCardsToMyCollection)]
        public async Task<ActionResult<CollectionDto>> AddCardsToMyCollection(
            long collectionUserId,
            short collectionId,
            [FromBody] AddCollectionsRequest request)
        {
            var userId = HttpContext.GetUserId();

            if (userId.IsFailed)
                return BadRequest();

            var collectionResult = await collectionService.AddCardsToMyCollection(
                UserId.Create(collectionUserId).Value,
                CollectionId.Create(collectionId).Value,
                userId.Value,
                //todo: check null
                CollectionId.Create(request.MyCollectionId.Value).Value,
            request.NewCollectionName,
                request.CheckUnique);
            
            return collectionResult.ToActionResult(collection => mapper.Map<CollectionDto>(collection));
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
