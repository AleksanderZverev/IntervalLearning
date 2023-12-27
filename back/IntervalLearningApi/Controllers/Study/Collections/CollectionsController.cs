using Application.Commands.Collections.AddPublicCollection;
using Application.Commands.Collections.CreateCollection;
using Application.Commands.Collections.GetAllUserCollections;
using Application.Commands.Collections.GetCanStartCollections;
using Application.Commands.Collections.GetCollection;
using Application.Commands.Collections.GetPublicCollection;
using Application.Commands.Collections.GetRandomWords;
using Application.Commands.Collections.GetRepeatCollections;
using Application.Commands.Collections.MakeCollectionPublic;
using Application.Commands.Collections.SearchCollection;
using Application.Commands.Collections.SearchPublicCollection;
using Application.Commands.Collections.UpdateCollection;
using Domain.Collection;
using Domain.Collection.ValueObjects;
using Domain.Schedule.ValueObjects;
using Domain.Theme.ValueObjects;
using Domain.User.ValueObjects;
using FluentResults;
using Infrastructure.Extensions;
using IntervalLearningApi.Constants;
using IntervalLearningApi.Controllers.Dictionary.DTOs;
using IntervalLearningApi.Controllers.Store.DTOs;
using IntervalLearningApi.Controllers.Study.Collections.DTOs;
using IntervalLearningApi.Controllers.Study.Collections.RequestModels.AddCardsToMyCollection;
using IntervalLearningApi.Controllers.Study.Collections.RequestModels.CreateCollection;
using IntervalLearningApi.Controllers.Study.Collections.RequestModels.GetNotFinished;
using IntervalLearningApi.Controllers.Study.Collections.RequestModels.GetRandomWords;
using IntervalLearningApi.Controllers.Study.Collections.RequestModels.GetRepeatCollections;
using IntervalLearningApi.Controllers.Study.RepeatsSchedules.DTOs;
using IntervalLearningApi.Extensions;
using IntervalLearningApi.Infrastructure.CommandManager;
using IntervalLearningApi.Infrastructure.ValidatorResolver;
using MapsterMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IntervalLearningApi.Controllers.Study.Collections
{
    [Route(ApiRoutes.Collections.BasePath)]
    [Authorize]
    [ApiController]
    public class CollectionsController : ControllerBase
    {
        private readonly ValidatorResolver validatorResolver;
        private readonly IMapper mapper;
        private readonly CommandManager commandManager;

        public CollectionsController(
            ValidatorResolver validatorResolver,
            IMapper mapper,
            CommandManager commandManager)
        {
            this.validatorResolver = validatorResolver;
            this.mapper = mapper;
            this.commandManager = commandManager;
        }

        [HttpPost(ApiRoutes.Collections.Create)]
        public async Task<ActionResult<CollectionDto>> CreateCollection(
            [FromBody] CreateCollectionRequest request)
        {
            var validation = validatorResolver.Validate(request);

            if (validation.IsFailed)
                return validation.ToErrorActionResult();
            
            var userId = HttpContext.GetUserId();

            if (userId.IsFailed)
                return BadRequest();

            Result<Collection> collectionResult;

            if (request.CollectionId == null)
            {
                collectionResult = await commandManager
                    .GetCommand<CreateCollectionCommand>()
                    .Handle(new CreateCollectionCommandRequest()
                    {
                        ParentUserId = userId.Value,
                        Title = CollectionTitle.Create(request.Title).Value,
                        ThemeId = ThemeId.Create(request.ThemeId).Value,
                        IsDefaultBackSide = request.IsDefaultBackSide,
                    });
            }
            else
            {
                collectionResult = await commandManager
                    .GetCommand<UpdateCollectionCommand>()
                    .Handle(new UpdateCollectionCommandRequest()
                    {
                        ParentUserId = userId.Value,
                        CollectionId = CollectionId.Create(request.CollectionId.Value).Value,
                        Title = CollectionTitle.Create(request.Title).Value,
                        ThemeId = ThemeId.Create(request.ThemeId).Value,
                        IsDefaultBackSide = request.IsDefaultBackSide,
                    });
            }

            return collectionResult.ToActionResult(collection => mapper.Map<CollectionDto>(collection));
        }

        [HttpGet(ApiRoutes.Collections.SearchPublic)]
        public async Task<ActionResult<List<StoreCollection>>> SearchPublicCollection(
            short themeId, 
            string? searchName = null,
            int page = 1,
            int count = 10)
        {
            var argsResult = (
                HttpContext.GetUserId(),
                ThemeId.Create(themeId)
            );

            if (argsResult.HasAnyError())
                return BadRequest();

            var (userId, themeIdResult) = argsResult;
            var collectionsResult = await commandManager
                .GetCommand<SearchPublicCollectionCommand>()
                .Handle(new SearchPublicCollectionCommandRequest(
                    userId.Value,
                    themeIdResult.Value,
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
        public async Task<ActionResult<List<CollectionDto>>> SearchCollection(
            short themeId, 
            string? searchName = null, 
            int page = 1, 
            int count = 10)
        {
            var argsResult = (
                HttpContext.GetUserId(),
                ThemeId.Create(themeId)
            );

            if (argsResult.HasAnyError())
                return BadRequest();

            var (userId, themeIdResult) = argsResult;
            var collectionsResult = await commandManager
                .GetCommand<SearchCollectionCommand>()
                .Handle(new SearchCollectionCommandRequest(
                    userId.Value,
                    themeIdResult.Value, 
                    searchName ?? "",
                    page,
                    count));

            return collectionsResult.ToActionResult(collections => mapper.Map<List<CollectionDto>>(collections));
        }

        [AllowAnonymous]
        [HttpGet(ApiRoutes.Collections.GetPublicCollection)]
        public async Task<ActionResult<CollectionDto>> GetPublicCollection(
            long userId, 
            short collectionId)
        {
            var argsResult = (
                UserId.Create(userId),
                CollectionId.Create(collectionId)
            );

            if (argsResult.HasAnyError())
                return BadRequest();

            var (userIdResult, collectionIdResult) = argsResult;
            var collectionResult = await commandManager
                .GetCommand<GetPublicCollectionCommand>()
                .Handle(new GetPublicCollectionCommandRequest(
                    userIdResult.Value,
                    collectionIdResult.Value));

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
                .Handle(new GetAllUserCollectionsCommandRequest(userId.Value));

            return collectionsResult.ToActionResult(collections => mapper.Map<List<CollectionDto>>(collections));
        }

        [HttpGet(ApiRoutes.Collections.GetRandomWords)]
        public async Task<ActionResult<GetRandomWordResponse>> GetRandomWords(
            [FromQuery]short collectionId)
        {
            var argsResult = (
                HttpContext.GetUserId(),
                CollectionId.Create(collectionId)
            );

            if (argsResult.HasAnyError())
                return BadRequest();

            var (userIdResult, collectionIdResult) = argsResult;
            var wordsResult = await commandManager
                .GetCommand<GetRandomWordsCommand>()
                .Handle(new GetRandomWordsCommandRequest(
                    userIdResult.Value, 
                    collectionIdResult.Value));

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
                .Handle(new GetRepeatCollectionsCommandRequest(userId.Value));

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
            var argsResult = (
                HttpContext.GetUserId(),
                UserId.Create(scheduleUserId),
                ScheduleId.Create(scheduleId)
            );

            if (argsResult.HasAnyError())
                return BadRequest();

            var (userId, scheduleUserIdResult, scheduleIdResult) = argsResult;
            var canStartCollectionsResult = await commandManager
                .GetCommand<GetCanStartCollectionsCommand>()
                .Handle(new GetCanStartCollectionsCommandRequest(
                    userId.Value,
                    scheduleUserIdResult.Value,
                    scheduleIdResult.Value,
                    page,
                    count));

            return canStartCollectionsResult.ToActionResult(response =>
                new GetNotFinishedResponse()
                {
                    TotalCollections = response.TotalCollections,
                    CanStartCollections = mapper.Map<List<CollectionDto>>(response.CanStartCollections),
                    CanRelearnCollections = mapper.Map<List<CollectionDto>>(response.CollectionsWithRelearningWords),
                });
        }

        [HttpGet(ApiRoutes.Collections.GetCollection)]
        public async Task<ActionResult<CollectionDto>> Get(short collectionId)
        {
            var argsResult = (
                HttpContext.GetUserId(),
                CollectionId.Create(collectionId)
            );

            if (argsResult.HasAnyError())
                return BadRequest();

            var (userId, collectionIdResult) = argsResult;
            var collectionResult = await commandManager
                .GetCommand<GetCollectionCommand>()
                .Handle(new GetCollectionCommandRequest(
                    userId.Value, 
                    collectionIdResult.Value));
            
            return collectionResult.ToActionResult(collection => mapper.Map<CollectionDto>(collection));
        }

        [HttpPost(ApiRoutes.Collections.MakePublic)]
        public async Task<ActionResult<CollectionDto>> MakePublic(short collectionId)
        {
            var argsResult = (
                HttpContext.GetUserId(),
                CollectionId.Create(collectionId)
            );

            if (argsResult.HasAnyError())
                return BadRequest();

            var (userId, collectionIdResult) = argsResult;
            var collectionResult = await commandManager
                .GetCommand<MakeCollectionPublicCommand>()
                .Handle(new MakeCollectionPublicCommandRequest(
                    userId.Value, 
                    collectionIdResult.Value));
            
            return collectionResult.ToActionResult(collection => mapper.Map<CollectionDto>(collection));
        }

        [HttpPost(ApiRoutes.Collections.AddCardsToMyCollection)]
        public async Task<ActionResult<CollectionDto>> AddCardsToMyCollection(
            long collectionUserId,
            short collectionId,
            [FromBody] AddCollectionsRequest request)
        {
            var validation = validatorResolver.Validate(request);

            if (validation.IsFailed)
                return validation.ToErrorActionResult();
            
            var argsResult = (
                HttpContext.GetUserId(),
                UserId.Create(collectionUserId),
                CollectionId.Create(collectionId)
            );

            if (argsResult.HasAnyError())
                return BadRequest();

            var (userId, collectionUserIdResult, collectionIdResult) = argsResult;
            var collectionResult = await commandManager
                .GetCommand<AddPublicCollectionCommand>()
                .Handle(new AddPublicCollectionCommandRequest(
                    collectionUserIdResult.Value,
                    collectionIdResult.Value,
                    userId.Value,
                    request.MyCollectionId == null 
                        ? null 
                        :CollectionId.Create(request.MyCollectionId.Value).Value,
                    request.NewCollectionName,
                    request.CheckUnique));
            
            return collectionResult.ToActionResult(collection => mapper.Map<CollectionDto>(collection));
        }
    }
}
