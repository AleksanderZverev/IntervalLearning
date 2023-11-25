using System.ComponentModel.DataAnnotations;
using Application.Commands.Cards;
using Application.Commands.Cards.CreateCard;
using Application.Commands.Cards.GetAllCards;
using Application.Commands.Cards.GetCardsQueueCommand;
using Application.Commands.Cards.GetNotStartedCardsCommand;
using Application.Commands.Cards.SearchCards;
using Application.Commands.Cards.UpdateCard;
using Application.Commands.Collections.DeleteCardFromCollection;
using Application.Commands.Collections.MoveCollectionCard;
using DB.Models.ValueObjects;
using Domain.Card.ValueObjects;
using Domain.Collection.ValueObjects;
using Domain.User.ValueObjects;
using IntervalLearningApi.Constants;
using IntervalLearningApi.Extensions;
using IntervalLearningApi.Infrastructure.CommandManager;
using IntervalLearningApi.Models.ByUser;
using IntervalLearningApi.Models.RepeatsSchedule;
using IntervalLearningApi.Services;
using MapsterMapper;
using Microsoft.AspNetCore.Mvc;
using SearchFieldType = IntervalLearningApi.Models.SearchFieldType;

namespace IntervalLearningApi.Controllers
{
    [Route(ApiRoutes.Cards.BasePath)]
    [Authorize]
    [ApiController]
    public partial class CardsController : ControllerBase
    {
        private readonly IMapper mapper;
        private readonly CommandManager commandManager;
        private readonly CardsService cardsService;
        private readonly CollectionService collectionService;
        private readonly IHostEnvironment env;

        public CardsController(
            IMapper mapper,
            CommandManager commandManager,
            CardsService cardsService, 
            CollectionService collectionService, 
            IHostEnvironment env)
        {
            this.mapper = mapper;
            this.commandManager = commandManager;
            this.cardsService = cardsService;
            this.collectionService = collectionService;
            this.env = env;
        }
        
        [HttpPost(ApiRoutes.Cards.Post_CreateCard)]
        public async Task<ActionResult<CardDto>> CreateCard(short collectionId, [FromBody]CreateCardItem item)
        {
            if (item.Examples != null && item.Examples.Any(e => e.Length > 255))
            {
                return BadRequest();
            }

            var userId = HttpContext.GetUserId();

            if (userId.IsFailed)
                return BadRequest();

            var collectionIdDomain = CollectionId.Create(collectionId).Value;

            if (item.CardId == null)
            {
                ;
                var createdResult = await commandManager
                    .GetCommand<CreateCardCommand>()
                    .Handle(new CreateCardRequest()
                {
                    ParentUserId = userId.Value,
                    ParentCollectionId = collectionIdDomain,
                    RememberingText = CardText.Create(item.FrontText).Value,
                    PromptText = item.PromptText == null ? null : CardText.Create(item.PromptText).Value,
                    MeaningText = CardText.Create(item.BackText).Value,
                    Description = item.Description != null ? CardDescription.Create(item.Description).Value : null,
                    Examples = item.Examples != null
                        ? item.Examples.Select(e => CardExample.Create(e).Value).ToList()
                        : new List<CardExample>()
                });
                
                return createdResult.ToActionResult(c => mapper.Map<CardDto>(c));
            }

            var cardResult = await commandManager
                .GetCommand<UpdateCardCommand>()
                .Handle(new UpdateCardRequest(){
                CardId = CardId.Create(item.CardId.Value).Value,
                ParentUserId = userId.Value,
                ParentCollectionId = collectionIdDomain,
                RememberingText = CardText.Create(item.FrontText).Value,
                PromptText = item.PromptText == null ? null : CardText.Create(item.PromptText).Value,
                MeaningText = CardText.Create(item.BackText).Value,
                Description = item.Description != null ? CardDescription.Create(item.Description).Value : null,
                Examples = item.Examples != null
                    ? item.Examples.Select(e => CardExample.Create(e).Value).ToList()
                    : new List<CardExample>()
            });
            
            return cardResult.ToActionResult(card => mapper.Map<CardDto>(card));
        }

        [HttpGet(ApiRoutes.Cards.Get_Card)]
        public async Task<ActionResult<CardDto>> GetCard(short collectionId, short cardId)
        {
            var userId = HttpContext.GetUserId();

            if (userId.IsFailed)
                return BadRequest();

            var cardResult = await commandManager
                .GetCommand<GetCardCommand>()
                .Handle(new GetCardRequest(
                    userId.Value,
                    CollectionId.Create(collectionId).Value,
                    CardId.Create(cardId).Value));
            
            return cardResult.ToActionResult(c => mapper.Map<CardDto>(c));
        }

        [HttpGet(ApiRoutes.Cards.Get_GetAll)]
        public async Task<ActionResult<List<CardDto>>> GetCards(
            short collectionId, 
            [FromQuery] int page = 1,
            [FromQuery] int count = 10)
        {
            var userId = HttpContext.GetUserId();

            if (userId.IsFailed)
                return BadRequest();

            var cardsResult = await commandManager
                .GetCommand<GetAllCardsCommand>()
                .Handle(new GetAllCardsRequest(
                    userId.Value,
                    CollectionId.Create(collectionId).Value,
                    page,
                    count));
            
            return cardsResult.ToActionResult(cards => mapper.Map<List<CardDto>>(cards));
        }

        [HttpGet(ApiRoutes.Cards.Get_GetCardQueue)]
        public async Task<ActionResult<List<CardDto>>> GetCardsQueue(
            short collectionId,
            [FromQuery] long scheduleUserId,
            [FromQuery] short scheduleId,
            [FromQuery] short phaseIndex,
            [FromQuery] DateTime date)
        {
            var userId = HttpContext.GetUserId();

            if (userId.IsFailed)
                return BadRequest();
            
            if (env.IsProduction() && date.Date > DateTime.UtcNow.Date)
                return new List<CardDto>();

            var cardsResult = await commandManager
                .GetCommand<GetCardsQueueCommand>()
                .Handle(new GetCardsQueueRequest(
                    userId.Value,
                    CollectionId.Create(collectionId).Value,
                    UserId.Create(scheduleUserId).Value,
                    ScheduleId.Create(scheduleId).Value,
                    phaseIndex,
                    date));

            return cardsResult.ToActionResult(cards => mapper.Map<List<CardDto>>(cards));
        }

        [HttpGet(ApiRoutes.Cards.Get_GetNotStartedCards)]
        public async Task<ActionResult<List<CardDto>>> GetNotStartedCards(
            short collectionId,
            long scheduleUserId,
            short scheduleId,
            [Range(1, 1000)]int count)
        {
            var userId = HttpContext.GetUserId();

            if (userId.IsFailed)
                return BadRequest();

            var cardsResult = await commandManager
                .GetCommand<GetNotStartedCardsCommand>()
                .Handle(new GetNotStartedCardsRequest(
                    UserId.Create(scheduleUserId).Value,
                    ScheduleId.Create(scheduleId).Value,
                    userId.Value,
                    CollectionId.Create(collectionId).Value,
                    count));

            return cardsResult.ToActionResult(cards => mapper.Map<List<CardDto>>(cards));
        }

        [HttpDelete(ApiRoutes.Cards.Delete_DeleteCard)]
        public async Task<ActionResult<CardDto>> DeleteCard(short collectionId, short cardId)
        {
            var userId = HttpContext.GetUserId();

            if (userId.IsFailed)
                return BadRequest();

            var cardResult = await commandManager
                .GetCommand<DeleteCardFromCollectionCommand>()
                .Handle(new DeleteCardFromCollectionRequest(
                    userId.Value,
                    CollectionId.Create(collectionId).Value,
                    CardId.Create(cardId).Value));
            
            return cardResult.ToActionResult(card => mapper.Map<CardDto>(card));
        }

        [HttpPost(ApiRoutes.Cards.Post_MoveCard)]
        public async Task<ActionResult<CardDto>> MoveCard(short collectionId, [FromBody] MoveRequest request)
        {
            var userId = HttpContext.GetUserId();

            if (userId.IsFailed)
                return BadRequest();

            var cardResult = await commandManager
                .GetCommand<MoveCollectionCardCommand>()
                .Handle(new MoveCollectionCardRequest(
                    userId.Value,
                    CollectionId.Create(collectionId).Value,
                    CollectionId.Create(request.DestinationCollectionId).Value,
                    CardId.Create(request.CardId).Value));
            
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
            var userId = HttpContext.GetUserId();

            if (userId.IsFailed)
                return BadRequest();

            var cardsResult = await commandManager
                .GetCommand<SearchCardsCommand>()
                .Handle(new SearchCardsRequest(
                    userId.Value,
                    CollectionId.Create(collectionId).Value,
                    searchValue.ToLower(),
                    (Application.Commands.Cards.SearchCards.SearchFieldType)fieldType,
                    page,
                    count));

            return cardsResult.ToActionResult(cards => mapper.Map<List<CardDto>>(cards));
        }

        [HttpPost(ApiRoutes.Cards.Post_StartCards)]
        public ActionResult<StartCardResponse> StartCards(short collectionId, [FromBody]CardsItem item)
        {
            var userId = HttpContext.GetUserId();

            if (userId.IsFailed)
                return BadRequest();

            var closestRepeatInfoResult = cardsService.Start(
                userId.Value,
                CollectionId.Create(collectionId).Value,
                UserId.Create(item.ScheduleUserId).Value,
                ScheduleId.Create(item.ScheduleId).Value, 
                item.CardIds);

            return closestRepeatInfoResult.ToActionResult(closestRepeatInfo =>
                new StartCardResponse(
                    closestRepeatInfo.NextRepeatDate,
                    closestRepeatInfo.NextPhase == null
                        ? null
                        : mapper.Map<PhaseDto>(closestRepeatInfo.NextPhase),
                    closestRepeatInfo.NextPhaseIndex));
        }

        [HttpPatch(ApiRoutes.Cards.Path_RememberCard)]
        public async Task<ActionResult<RememberCardResponse>> RememberCard(short collectionId, [FromBody] RememberRequest request)
        {
            var userId = HttpContext.GetUserId();

            if (userId.IsFailed)
                return BadRequest();
            
            var closestRepeatInfoResult = await cardsService.Remember(
                userId.Value,
                CollectionId.Create(collectionId).Value,
                UserId.Create(request.ScheduleUserId).Value,
                ScheduleId.Create(request.ScheduleId).Value,
                request.PhaseIndex,
                ToCardServiceRememberItems(request.RememberItems)
            );

            return closestRepeatInfoResult.ToActionResult(closestRepeatInfo =>
                new RememberCardResponse(
                    closestRepeatInfo.NextRepeatDate,
                    closestRepeatInfo.NextPhase == null
                        ? null
                        : mapper.Map<PhaseDto>(closestRepeatInfo.NextPhase),
                    closestRepeatInfo.NextPhaseIndex));
        }

        private List<CardsService.RememberItem> ToCardServiceRememberItems(List<RememberItemDto> requestRememberItems)
        {
            return requestRememberItems.Select(r => new CardsService.RememberItem
            {
                CardId = CardId.Create(r.CardId).Value,
                Weight = RememberWeight.Create(r.Weight).Value,
            }).ToList();
        }
    }

    public class StartCardResponse
    {
        public DateTime? NextRepeatDate { get; }
        public PhaseDto? NextRepeatPhase { get; }
        public int NextPhaseIndex { get; }

        public StartCardResponse(DateTime? nextRepeatDate, PhaseDto? nextRepeatPhase, int nextPhaseIndex)
        {
            NextRepeatDate = nextRepeatDate;
            NextRepeatPhase = nextRepeatPhase;
            NextPhaseIndex = nextPhaseIndex;
        }
    }

    public class RememberCardResponse
    {
        public DateTime? NextRepeatDate { get; }
        public PhaseDto? NextRepeatPhase { get; }
        public int NextPhaseIndex { get; }

        public RememberCardResponse(DateTime? nextRepeatDate, PhaseDto? nextRepeatPhase, int nextPhaseIndex)
        {
            NextRepeatDate = nextRepeatDate;
            NextRepeatPhase = nextRepeatPhase;
            NextPhaseIndex = nextPhaseIndex;
        }
    }

    public class CardsItem
    {
        public long ScheduleUserId { get; set; }
        public short ScheduleId { get; set; }
        public List<short> CardIds { get; set; }
    }

    public class RememberRequest
    {
        public List<RememberItemDto> RememberItems { get; set; }
        public long ScheduleUserId { get; set; }
        public short ScheduleId { get; set; }
        public short PhaseIndex { get; set; }
    }

    public class MoveRequest
    {
        public short DestinationCollectionId { get; set; }
        public short CardId { get; set; }
    }

    public class RememberItemDto
    {
        public short CardId { get; set; }
        public float Weight { get; set; }
    }

    public class CreateCardItem
    {
        public short? CardId { get; set; }
        [Required]
        [StringLength(255)]
        public string FrontText { get; set; }

        [StringLength(255)] 
        public string? PromptText { get; set; }

        [Required]
        [StringLength(255)]
        public string BackText { get; set; }
        [StringLength(500)]
        public string? Description { get; set; }

        [MaxLength(15)]
        public List<string>? Examples { get; set; }
    }
}
