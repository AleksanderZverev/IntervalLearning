using Application.Commands.Collections.AddPublicCollection;
using Application.Commands.Collections.CreateCollection;
using Application.Commands.Collections.GetAll;
using Application.Commands.Collections.GetCanStartCollections;
using Application.Commands.Collections.GetCollection;
using Application.Commands.Collections.GetPublicCollection;
using Application.Commands.Collections.GetRandomWords;
using Application.Commands.Collections.GetRepeatCollections;
using Application.Commands.Collections.MakeCollectionPublic;
using Application.Commands.Collections.SearchCollection;
using Application.Commands.Collections.SearchPublicCollection;
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
                        Title = CollectionTitle.Create(item.Title).Value,
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
                        Title = CollectionTitle.Create(item.Title).Value,
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

            var collectionsResult = await commandManager
                .GetCommand<SearchPublicCollectionCommand>()
                .Handle(new SearchPublicCollectionRequest(
                    userId.Value,
                    ThemeId.Create(themeId).Value,
                    searchName ?? "",
                    page,
                    count));
            
            return collectionsResult.ToActionResult(collections => 
                mapper.Map<List<StoreCollection>>(
                    collections.Select(item => (item.Collection, item.Subscriber))
                    )
                );
        }

        [HttpGet(ApiRoutes.Collections.SearchPrivate)]
        public async Task<ActionResult<List<CollectionDto>>> SearchCollection(short themeId, string? searchName = null, int page = 1, int count = 10)
        {
            var userId = HttpContext.GetUserId();

            if (userId.IsFailed)
                return BadRequest();

            var collectionsResult = await commandManager
                .GetCommand<SearchCollectionCommand>()
                .Handle(new SearchCollectionRequest(
                    userId.Value, ThemeId.Create(themeId).Value, searchName ?? "", page, count));

            return collectionsResult.ToActionResult(collections => mapper.Map<List<CollectionDto>>(collections));
        }

        [AllowAnonymous]
        [HttpGet(ApiRoutes.Collections.GetPublicCollection)]
        public async Task<ActionResult<CollectionDto>> GetPublicCollection(long userId, short collectionId)
        {
            var collectionResult = await commandManager
                .GetCommand<GetPublicCollectionCommand>()
                .Handle(new GetPublicCollectionRequest(
                    UserId.Create(userId).Value,
                    CollectionId.Create(collectionId).Value));

            return collectionResult.ToActionResult(collection => mapper.Map<CollectionDto>(collection));
        }

        [HttpGet(ApiRoutes.Collections.GetAll)]
        public async Task<ActionResult<List<CollectionDto>>> GetAll()
        {
            var userId = HttpContext.GetUserId();

            if (userId.IsFailed)
                return BadRequest();

            var collectionsResult = await commandManager
                .GetCommand<GetAllUserCollectionsCommand>()
                .Handle(new GetAllUserCollectionsRequest(userId.Value));

            return collectionsResult.ToActionResult(collections => mapper.Map<List<CollectionDto>>(collections));
        }

        [HttpGet(ApiRoutes.Collections.GetRandomWords)]
        public async Task<ActionResult<GetRandomWordResponse>> GetRandomWords([FromQuery]short collectionId)
        {
            var userId = HttpContext.GetUserId();

            if (userId.IsFailed)
                return BadRequest();

            var wordsResult = await commandManager
                .GetCommand<GetRandomWordsCommand>()
                .Handle(new GetRandomWordsRequest(userId.Value, CollectionId.Create(collectionId).Value));

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

            var dateToRepeatingCollectionsResult = await commandManager
                .GetCommand<GetRepeatCollectionsCommand>()
                .Handle(new GetRepeatCollectionsRequest(userId.Value));

            return dateToRepeatingCollectionsResult.ToActionResult(dateToRepeatingCollections =>
                new RepeatingCollectionResponse(dateToRepeatingCollections
                    .ToDictionary(
                        p => p.Key,
                        p => p.Value.Select(c => mapper.Map<RepeatingPhaseDto>(c)).ToList())));
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

            var canStartCollectionsResult = await commandManager
                .GetCommand<GetCanStartCollectionsCommand>()
                .Handle(new GetCanStartCollectionsRequest(
                    userId.Value,
                    UserId.Create(scheduleUserId).Value,
                    ScheduleId.Create(scheduleId).Value,
                    page,
                    count));

            return canStartCollectionsResult.ToActionResult(response =>
                new GetNotFinishedResponse(
                    response.TotalCollections,
                    mapper.Map<List<CollectionDto>>(response.CanStartCollections)));
        }

        [HttpGet(ApiRoutes.Collections.GetCollection)]
        public async Task<ActionResult<CollectionDto>> Get(short collectionId)
        {
            var userId = HttpContext.GetUserId();

            if (userId.IsFailed)
                return BadRequest();

            var collectionResult = await commandManager
                .GetCommand<GetCollectionCommand>()
                .Handle(new GetCollectionRequest(userId.Value, CollectionId.Create(collectionId).Value));
            
            return collectionResult.ToActionResult(collection => mapper.Map<CollectionDto>(collection));
        }

        [HttpPost(ApiRoutes.Collections.MakePublic)]
        public async Task<ActionResult<CollectionDto>> MakePublic(short collectionId)
        {
            var userId = HttpContext.GetUserId();

            if (userId.IsFailed)
                return BadRequest();

            var collectionResult = await commandManager
                .GetCommand<MakeCollectionPublicCommand>()
                .Handle(new MakeCollectionPublicRequest(userId.Value, CollectionId.Create(collectionId).Value));
            
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

            var collectionResult = await commandManager
                .GetCommand<AddPublicCollectionCommand>()
                .Handle(new AddPublicCollectionRequest(
                    UserId.Create(collectionUserId).Value,
                    CollectionId.Create(collectionId).Value,
                    userId.Value,
                    //todo: check null
                    CollectionId.Create(request.MyCollectionId.Value).Value,
                    request.NewCollectionName,
                    request.CheckUnique));
            
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
