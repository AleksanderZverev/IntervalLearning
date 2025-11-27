using System.ComponentModel.DataAnnotations;
using Application.Commands.Cards.CreateCard;
using Application.Commands.Cards.DeleteCard;
using Application.Commands.Cards.GetAllCards;
using Application.Commands.Cards.GetCard;
using Application.Commands.Cards.GetCardsQueueCommand;
using Application.Commands.Cards.GetNotStartedCardsCommand;
using Application.Commands.Cards.GetRelearningCards;
using Application.Commands.Cards.PostponeRepeatingCard;
using Application.Commands.Cards.RelearnCard;
using Application.Commands.Cards.RememberCard;
using Application.Commands.Cards.SearchCards;
using Application.Commands.Cards.StartLearnCards;
using Application.Commands.Cards.StopRepeatingCard;
using Application.Commands.Cards.UpdateCard;
using Application.Commands.Collections.MoveCollectionCard;
using Domain.Card.ValueObjects;
using Domain.Collection.ValueObjects;
using Domain.Common.ValueObjects.Text.SingleLine;
using Domain.Schedule.Entities.Remember.ValueObjects;
using Domain.Schedule.ValueObjects;
using Domain.User.ValueObjects;
using GlobalTools.Errors;
using GlobalTools.Extensions;
using IntervalLearningApi.Constants;
using IntervalLearningApi.Controllers.Study.Cards.DTOs;
using IntervalLearningApi.Controllers.Study.Cards.Requests;
using IntervalLearningApi.Controllers.Study.Cards.Requests.RememberCard;
using IntervalLearningApi.Controllers.Study.Cards.Requests.StartCards;
using IntervalLearningApi.Controllers.Study.Cards.Responses.GetRepeatingCardsForDate;
using IntervalLearningApi.Controllers.Study.RepeatsSchedules.DTOs;
using IntervalLearningApi.Extensions;
using IntervalLearningApi.Infrastructure.CommandManager;
using IntervalLearningApi.Infrastructure.ValidatorResolver;
using MapsterMapper;
using Microsoft.AspNetCore.Mvc;
using CreateCardRequest = IntervalLearningApi.Controllers.Study.Cards.Requests.CreateCardRequest;
using RememberCardRequest = IntervalLearningApi.Controllers.Study.Cards.Requests.RememberCard.RememberCardRequest;
using SearchFieldType = IntervalLearningApi.Controllers.Study.Cards.DTOs.SearchFieldType;

namespace IntervalLearningApi.Controllers.Study.Cards
{
    [Route(ApiRoutes.Cards.BasePath)]
    [Authorize]
    [ApiController]
    public class CardsController : ControllerBase
    {
        private readonly ValidatorResolver validatorResolver;
        private readonly IMapper mapper;
        private readonly CommandManager commandManager;
        private readonly IHostEnvironment env;

        public CardsController(
            ValidatorResolver validatorResolver,
            IMapper mapper,
            CommandManager commandManager,
            IHostEnvironment env)
        {
            this.validatorResolver = validatorResolver;
            this.mapper = mapper;
            this.commandManager = commandManager;
            this.env = env;
        }
        
        [HttpPost(ApiRoutes.Cards.Post_CreateCard)]
        public async Task<ActionResult<CardDto>> CreateCard(short collectionId, [FromBody]CreateCardRequest request)
        {
            var validationResult = validatorResolver.Validate(request);

            if (validationResult.IsFailed)
            {
                return validationResult.ToErrorActionResult();
            }

            var userId = HttpContext.GetUserId();

            if (userId.IsFailed)
            {
                return BadRequest();
            }

            var collectionIdDomain = CollectionId.Create(collectionId).Value;

            if (request.CardId == null)
            {
                var createdResult = await commandManager
                    .GetCommand<CreateCardCommand>()
                    .Handle(new Application.Commands.Cards.CreateCard.CreateCardRequest() 
                    {
                        ParentUserId = userId.Value,
                        ParentCollectionId = collectionIdDomain,
                        RememberingText = CardText.Create(request.FrontText).Value,
                        PromptText = string.IsNullOrWhiteSpace(request.PromptText) ? null : CardText.Create(request.PromptText).Value,
                        MeaningText = CardText.Create(request.BackText).Value,
                        Description = string.IsNullOrWhiteSpace(request.Description) ? null : CardDescription.Create(request.Description).Value,
                        Examples = request.Examples != null
                            ? request.Examples.Select(e => CardExample.Create(e).Value).ToList()
                            : new List<CardExample>(),
                        Tags = request.Tags != null
                            ? request.Tags.Select(t => CardTag.Create(t).Value).ToList()
                            : new List<CardTag>(),
                    });
                
                return createdResult.ToActionResult(c => mapper.Map<CardDto>(c));
            }

            var cardResult = await commandManager
                .GetCommand<UpdateCardCommand>()
                .Handle(new UpdateCardRequest()
                {
                    CardId = CardId.Create(request.CardId.Value).Value,
                    ParentUserId = userId.Value,
                    ParentCollectionId = collectionIdDomain,
                    RememberingText = CardText.Create(request.FrontText).Value,
                    PromptText = string.IsNullOrWhiteSpace(request.PromptText) ? null : CardText.Create(request.PromptText).Value,
                    MeaningText = CardText.Create(request.BackText).Value,
                    Description =string.IsNullOrWhiteSpace(request.Description) ? null : CardDescription.Create(request.Description).Value,
                    Examples = request.Examples != null
                        ? request.Examples.Select(e => CardExample.Create(e).Value).ToList()
                        : new List<CardExample>(),
                    Tags = request.Tags != null
                        ? request.Tags.Select(t => CardTag.Create(t).Value).ToList()
                        : new List<CardTag>(),
                });
            
            return cardResult.ToActionResult(card => mapper.Map<CardDto>(card));
        }

        [HttpGet(ApiRoutes.Cards.Get_Card)]
        public async Task<ActionResult<CardDto>> GetCard(short collectionId, short cardId)
        {
            var (
                userId,
                collectionIdResult,
                cardIdResult
                ) = (
                HttpContext.GetUserId(),
                CollectionId.Create(collectionId),
                CardId.Create(cardId)
            );


            if (userId.IsFailed || collectionIdResult.IsFailed || cardIdResult.IsFailed)
                return BadRequest();

            var cardResult = await commandManager
                .GetCommand<GetCardCommand>()
                .Handle(new GetCardRequest(
                    userId.Value,
                    collectionIdResult.Value,
                    cardIdResult.Value));
            
            return cardResult.ToActionResult(c => mapper.Map<CardDto>(c));
        }

        [HttpGet(ApiRoutes.Cards.Get_GetAll)]
        public async Task<ActionResult<List<CardDto>>> GetCards(
            short collectionId, 
            [FromQuery] int page = 1,
            [FromQuery] int count = 10)
        {
            var argsResult = (
                HttpContext.GetUserId(),
                CollectionId.Create(collectionId)
            );

            if (argsResult.HasAnyError())
                return BadRequest();

            var (userId, collectionIdResult) = argsResult;
            
            var cardsResult = await commandManager
                .GetCommand<GetAllCardsCommand>()
                .Handle(new GetAllCardsRequest(
                    userId.Value,
                    collectionIdResult.Value,
                    page,
                    count));
            
            return cardsResult.ToActionResult(cards => mapper.Map<List<CardDto>>(cards));
        }

        [HttpDelete(ApiRoutes.Cards.Delete_StopRepeatingCard)]
        public async Task<ActionResult> StopRepeatingCard(
            short collectionId,
            short cardId,
            [FromQuery] long scheduleUserId,
            [FromQuery] short scheduleId)
        {
            var argResults = (
                HttpContext.GetUserId(),
                CollectionId.Create(collectionId),
                CardId.Create(cardId),
                UserId.Create(scheduleUserId),
                ScheduleId.Create(scheduleId)
            );

            if (argResults.HasAnyError())
                return BadRequest();

            var (userIdResult, collectionIdResult, cardIdResult, scheduleUserIdResult, scheduleIdResult) = argResults;

            var stopResult = await commandManager
                .GetCommand<StopRepeatingCardCommand>()
                .Handle(new StopRepeatingCardCommandRequest(
                    userIdResult.Value, collectionIdResult.Value, cardIdResult.Value,
                    scheduleUserIdResult.Value, scheduleIdResult.Value));

            return stopResult.ToActionResult();
        }
        
        [HttpPatch(ApiRoutes.Cards.Patch_PostponeRepeatingCard)]
        public async Task<ActionResult> PostponeRepeatingCard(
            short collectionId,
            short cardId,
            [FromQuery] long scheduleUserId,
            [FromQuery] short scheduleId,
            [FromQuery, Range(1, 14)] int postponeDays)
        {
            var argResults = (
                HttpContext.GetUserId(),
                CollectionId.Create(collectionId),
                CardId.Create(cardId),
                UserId.Create(scheduleUserId),
                ScheduleId.Create(scheduleId)
            );

            if (argResults.HasAnyError())
                return BadRequest();

            var (userIdResult, collectionIdResult, cardIdResult, scheduleUserIdResult, scheduleIdResult) = argResults;

            var postponeResult = await commandManager
                .GetCommand<PostponeRepeatingCardCommand>()
                .Handle(new PostponeRepeatingCardCommandRequest(
                    userIdResult.Value, collectionIdResult.Value, cardIdResult.Value,
                    scheduleUserIdResult.Value, scheduleIdResult.Value,
                    postponeDays,
                    !env.IsProduction()
                ));

            return postponeResult.ToActionResult();
        }

        [HttpGet(ApiRoutes.Cards.Get_GetRepeatingCardsForDate)]
        public async Task<ActionResult<GetRepeatingCardsForDateResponse>> GetRepeatingCardsForDate(
            short collectionId,
            [FromQuery] int page,
            [FromQuery] int count,
            [FromQuery] long scheduleUserId,
            [FromQuery] short scheduleId,
            [FromQuery] bool isRepeatingMode,
            [FromQuery] DateTime date,
            [FromQuery] DateTimeOffset userCurrentDateTime)
        {
            var argResults = (
                page > 0 ? null : new BadRequestError("Incorrect page"),
                count > 0 ? null : new BadRequestError("Incorrect count"),
                HttpContext.GetUserId(),
                CollectionId.Create(collectionId),
                UserId.Create(scheduleUserId),
                ScheduleId.Create(scheduleId)
            );

            if (argResults.HasAnyError())
                return BadRequest();

            var (
                _,
                __,
                userId,
                collectionIdResult,
                scheduleUserIdResult,
                scheduleIdResult
                ) = argResults;
            
            var cardsResult = await commandManager
                .GetCommand<GetCardsQueueCommand>()
                .Handle(new GetCardsQueueRequest(
                    page,
                    count,
                    userId.Value,
                    collectionIdResult.Value,
                    scheduleUserIdResult.Value,
                    scheduleIdResult.Value,
                    isRepeatingMode,
                    date,
                    env.IsProduction(),
                    userCurrentDateTime));

            return cardsResult.ToActionResult(response => mapper.Map<GetRepeatingCardsForDateResponse>(response));
        }

        [HttpGet(ApiRoutes.Cards.Get_GetNotStartedCards)]
        public async Task<ActionResult<List<CardDto>>> GetNotStartedCards(
            short collectionId,
            long scheduleUserId,
            short scheduleId,
            [Range(1, 1000)]int count)
        {
            var argsResults = (
                UserId.Create(scheduleUserId),
                ScheduleId.Create(scheduleId),
                HttpContext.GetUserId(),
                CollectionId.Create(collectionId)
            );

            if (argsResults.HasAnyError())
                return BadRequest();

            var (scheduleUserIdResult, scheduleIdResult, userId, collectionIdResult) = argsResults;
            var cardsResult = await commandManager
                .GetCommand<GetNotStartedCardsCommand>()
                .Handle(new GetNotStartedCardsRequest(
                    scheduleUserIdResult.Value,
                    scheduleIdResult.Value,
                    userId.Value,
                    collectionIdResult.Value,
                    count));

            return cardsResult.ToActionResult(cards => mapper.Map<List<CardDto>>(cards));
        }

        [HttpGet(ApiRoutes.Cards.Get_GetAllRelearningCards)]
        public async Task<ActionResult<List<CardDto>>> GetRelearningCard(
            short collectionId,
            [Range(1, 200)]int count)
        {
            var argsResults = (
                HttpContext.GetUserId(),
                CollectionId.Create(collectionId)
            );

            if (argsResults.HasAnyError())
                return BadRequest();

            var (userIdResult, collectionIdResult) = argsResults;
            var cardsResult = await commandManager
                .GetCommand<GetRelearningCardsCommand>()
                .Handle(new GetRelearningCardsCommandRequest(userIdResult.Value, collectionIdResult.Value, count));
            
            return cardsResult.ToActionResult(cards => mapper.Map<List<CardDto>>(cards));
        }

        [HttpPatch(ApiRoutes.Cards.Patch_RelearnCard)]
        public async Task<ActionResult> RelearnCard(
            short collectionId,
            short cardId,
            long? scheduleUserId = null,
            short? scheduleId = null)
        {
            var argsResults = (
                HttpContext.GetUserId(),
                CollectionId.Create(collectionId),
                CardId.Create(cardId),
                scheduleUserId.HasValue ? UserId.Create(scheduleUserId.Value) : null,
                scheduleId.HasValue ? ScheduleId.Create(scheduleId.Value) : null
            );
            
            if (argsResults.HasAnyError())
                return BadRequest();

            var (userIdResult, collectionIdResult, cardIdResult, scheduleUserIdResult, scheduleIdResult) = argsResults;
            var relearnResult = await commandManager
                .GetCommand<RelearnCardCommand>()
                .Handle(new RelearnCardCommandRequest(
                    userIdResult.Value,
                    collectionIdResult.Value,
                    cardIdResult.Value,
                    scheduleUserIdResult?.Value,
                    scheduleIdResult?.Value));

            return relearnResult.ToActionResult();
        }

        [HttpDelete(ApiRoutes.Cards.Delete_DeleteCard)]
        public async Task<ActionResult<CardDto>> DeleteCard(
            short collectionId,
            short cardId)
        {
            var argsResult = (
                HttpContext.GetUserId(),
                CollectionId.Create(collectionId),
                CardId.Create(cardId)
            );

            if (argsResult.HasAnyError())
                return BadRequest();

            var (userId, collectionIdResult, cardIdResult) = argsResult;
            var cardResult = await commandManager
                .GetCommand<DeleteCardCommand>()
                .Handle(new DeleteCardRequest(
                    userId.Value,
                    collectionIdResult.Value,
                    cardIdResult.Value));
            
            return cardResult.ToActionResult(card => mapper.Map<CardDto>(card));
        }

        [HttpPost(ApiRoutes.Cards.Post_MoveCard)]
        public async Task<ActionResult<CardDto>> MoveCard(
            short collectionId,
            [FromBody] MoveCardRequest cardRequest)
        {
            var validation = validatorResolver.Validate(cardRequest);

            if (validation.IsFailed)
                return validation.ToErrorActionResult();
            
            var argsResult = (
                HttpContext.GetUserId(),
                CollectionId.Create(collectionId),
                CollectionId.Create(cardRequest.DestinationCollectionId),
                CardId.Create(cardRequest.CardId)
            );

            if (argsResult.HasAnyError())
                return BadRequest();

            var (userId, collectionIdResult, destinationCollectionIdResult, cardIdResult) = argsResult;
            var cardResult = await commandManager
                .GetCommand<MoveCollectionCardCommand>()
                .Handle(new MoveCollectionCardRequest(
                    userId.Value,
                    collectionIdResult.Value,
                    destinationCollectionIdResult.Value,
                    cardIdResult.Value));
            
            return cardResult.ToActionResult(card => mapper.Map<CardDto>(card));
        }

        [HttpGet(ApiRoutes.Cards.Get_SearchCard)]
        public async Task<ActionResult<List<CardDto>>> SearchCard(
            short collectionId,
            [FromQuery] string searchValue,
            [FromQuery] SearchFieldType fieldType,
            [FromQuery] int page = 1,
            [FromQuery] int count = 10)
        {
            var argsResult = (
                HttpContext.GetUserId(),
                CollectionId.Create(collectionId)
            );

            if (argsResult.HasAnyError())
                return BadRequest();

            var (userId, collectionIdResult) = argsResult;
            var cardsResult = await commandManager
                .GetCommand<SearchCardsCommand>()
                .Handle(new SearchCardsRequest(
                    userId.Value,
                    collectionIdResult.Value,
                    searchValue.ToLower(),
                    (DomainServices.DB.Queries.Study.Cards.SearchFieldType)fieldType,
                    page,
                    count));

            return cardsResult.ToActionResult(cards => mapper.Map<List<CardDto>>(cards));
        }

        [HttpPost(ApiRoutes.Cards.Post_StartCards)]
        public async Task<ActionResult<StartCardsResponse>> StartCards(
            short collectionId,
            [FromBody]StartCardsRequest request)
        {
            var validation = validatorResolver.Validate(request);

            if (validation.IsFailed)
                return validation.ToErrorActionResult();
            
            var argsResult = (
                HttpContext.GetUserId(),
                CollectionId.Create(collectionId)
            );

            if (argsResult.HasAnyError())
                return BadRequest();

            var (userId, collectionIdResult) = argsResult;
            var closestRepeatInfoResult = await commandManager
                .GetCommand<StartLearnCardsCommand>()
                .Handle(new StartLearnCardsRequest(
                    userId.Value,
                    collectionIdResult.Value,
                    UserId.Create(request.ScheduleUserId).Value,
                    ScheduleId.Create(request.ScheduleId).Value, 
                    request.CardIds.Select(cId => CardId.Create(cId).Value).ToList()));

            return closestRepeatInfoResult.ToActionResult(closestRepeatInfo =>
                new StartCardsResponse()
                {
                    NextRepeatDate = closestRepeatInfo.NextRepeatDate,
                    NextPhaseIndex = closestRepeatInfo.NextPhaseIndex,
                    NextRepeatPhase = closestRepeatInfo.NextPhase == null
                        ? null
                        : mapper.Map<PhaseDto>(closestRepeatInfo.NextPhase),
                    CardMovementInfos = mapper.Map<List<CardMovementInfoDto>>(closestRepeatInfo.CardMovementInfos),
                });
        }

        [HttpPatch(ApiRoutes.Cards.Path_RememberCard)]
        public async Task<ActionResult<RememberCardResponse>> RememberCard(
            short collectionId, 
            [FromBody] RememberCardRequest cardRequest)
        {
            var validation = validatorResolver.Validate(cardRequest);

            if (validation.IsFailed)
                return validation.ToErrorActionResult();
            
            var argsResult = (
                HttpContext.GetUserId(),
                CollectionId.Create(collectionId)
            );

            if (argsResult.HasAnyError())
                return BadRequest();

            var (userId, collectionIdResult) = argsResult;
            var closestRepeatInfoResult = await commandManager
                .GetCommand<RememberCardCommand>()
                .Handle(new Application.Commands.Cards.RememberCard.RememberCardRequest(
                    userId.Value,
                    collectionIdResult.Value,
                    UserId.Create(cardRequest.ScheduleUserId).Value,
                    ScheduleId.Create(cardRequest.ScheduleId).Value,
                    ToCardServiceRememberItems(cardRequest.RememberItems),
                    !env.IsProduction(),
                    cardRequest.UserCurrentDateTime));

            return closestRepeatInfoResult.ToActionResult(closestRepeatInfo =>
                new RememberCardResponse()
                {
                    NextRepeatDate = closestRepeatInfo.NextRepeatDate,
                    NextPhaseIndex = closestRepeatInfo.NextPhaseIndex,
                    NextRepeatPhase = closestRepeatInfo.NextPhase == null
                        ? null
                        : mapper.Map<PhaseDto>(closestRepeatInfo.NextPhase),
                    CardMovementInfos = mapper.Map<List<CardMovementInfoDto>>(closestRepeatInfo.CardMovementInfos),
                });
        }

        private List<RememberItem> ToCardServiceRememberItems(List<RememberItemDto> requestRememberItems)
        {
            return requestRememberItems.Select(r => new RememberItem
            {
                CardId = CardId.Create(r.CardId).Value,
                Weight = RememberWeight.Create(r.Weight).Value,
                Comment = string.IsNullOrWhiteSpace(r.Comment) 
                    ? null 
                    : MediumSingleLineString.Create(r.Comment).Value,
            }).ToList();
        }
    }
}
