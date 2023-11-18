using System.ComponentModel.DataAnnotations;
using DB.Models.ValueObjects;
using Domain.Card.ValueObjects;
using Domain.Collection.ValueObjects;
using Domain.User.ValueObjects;
using IntervalLearningApi.Constants;
using IntervalLearningApi.Extensions;
using IntervalLearningApi.Models;
using IntervalLearningApi.Models.ByUser;
using IntervalLearningApi.Models.RepeatsSchedule;
using IntervalLearningApi.Services;
using MapsterMapper;
using Microsoft.AspNetCore.Mvc;

namespace IntervalLearningApi.Controllers
{
    [Route(ApiRoutes.Cards.BasePath)]
    [Authorize]
    [ApiController]
    public class CardsController : ControllerBase
    {
        private readonly IMapper mapper;
        private readonly CardsService cardsService;
        private readonly CollectionService collectionService;

        public CardsController(
            IMapper mapper,
            CardsService cardsService, CollectionService collectionService)
        {
            this.mapper = mapper;
            this.cardsService = cardsService;
            this.collectionService = collectionService;
        }

        [HttpGet(ApiRoutes.Cards.Get_Card)]
        public async Task<ActionResult<CardDto>> GetCard(short collectionId, short cardId)
        {
            var userId = HttpContext.GetUserId();

            if (userId.IsFailed)
                return BadRequest();

            var card = await cardsService.FindCard(userId.Value, CollectionId.Create(collectionId).Value, CardId.Create(cardId).Value);
            return card == null 
                ? NotFound()
                : mapper.Map<CardDto>(card);
        }

        [HttpGet(ApiRoutes.Cards.Get_GetAll)]
        public async Task<ActionResult<IList<CardDto>>> GetCards(short collectionId, [FromQuery] int page = 1, [FromQuery] int count = 10)
        {
            var userId = HttpContext.GetUserId();

            if (userId.IsFailed)
                return BadRequest();


            var cards = await cardsService.GetCards(userId.Value, CollectionId.Create(collectionId).Value, page, count);
            return mapper.Map<List<CardDto>>(cards);
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

            var cards = await cardsService.GetCardsQueue(
                userId.Value,
                CollectionId.Create(collectionId).Value,
                UserId.Create(scheduleUserId).Value,
                ScheduleId.Create(scheduleId).Value,
                phaseIndex,
                date);
            
            return mapper.Map<List<CardDto>>(cards);
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
            
            var (cards, error) = await cardsService.GetNotStartedCards(
                UserId.Create(scheduleUserId).Value,
                ScheduleId.Create(scheduleId).Value,
                userId.Value,
                CollectionId.Create(collectionId).Value,
                count);
            
            return cards == null 
                ? BadRequest(error) 
                : mapper.Map<List<CardDto>>(cards);
        }

        [HttpPost(ApiRoutes.Cards.Post_CreateCard)]
        public ActionResult<CardDto> CreateCard(short collectionId, [FromBody]CreateCardItem item)
        {
            if (item.Examples != null && item.Examples.Any(e => e.Length > 255))
            {
                return BadRequest();
            }

            var userId = HttpContext.GetUserId();

            if (userId.IsFailed)
                return BadRequest();


            var (card, error) = collectionService.CreateOrEditCard(
                userId.Value,
                CollectionId.Create(collectionId).Value,
                item.CardId == null 
                    ? null 
                    : CardId.Create(item.CardId.Value).Value,
                CardText.Create(item.FrontText).Value,
                item.PromptText == null 
                    ? null 
                    : CardText.Create(item.PromptText).Value,
                CardText.Create(item.BackText).Value,
                item.Description != null 
                    ? CardDescription.Create(item.Description).Value 
                    : null,
                item.Examples != null
                    ? item.Examples.Select(e => CardExample.Create(e).Value).ToList()
                    : new List<CardExample>());

            return card != null
                ? mapper.Map<CardDto>(card)
                : BadRequest(error);
        }

        [HttpDelete(ApiRoutes.Cards.Delete_DeleteCard)]
        public async Task<ActionResult<CardDto>> DeleteCard(short collectionId, short cardId)
        {
            var userId = HttpContext.GetUserId();

            if (userId.IsFailed)
                return BadRequest();

            var (cardEntity, error) = await collectionService.DeleteCard(userId.Value, CollectionId.Create(collectionId).Value, CardId.Create(cardId).Value);
            return cardEntity != null 
                ? mapper.Map<CardDto>(cardEntity)
                : BadRequest(error);
        }

        [HttpPost(ApiRoutes.Cards.Post_MoveCard)]
        public async Task<ActionResult<CardDto>> MoveCard(short collectionId, [FromBody] MoveRequest request)
        {
            var userId = HttpContext.GetUserId();

            if (userId.IsFailed)
                return BadRequest();

            var (cardEntity, error) = await collectionService.MoveCard(
                userId.Value,
                CollectionId.Create(collectionId).Value,
                CollectionId.Create(request.DestinationCollectionId).Value,
                CardId.Create(request.CardId).Value);
            
            return cardEntity != null 
                ? mapper.Map<CardDto>(cardEntity) 
                : BadRequest(error);
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

            var cardEntities = await cardsService.Search(userId.Value, CollectionId.Create(collectionId).Value, searchValue.ToLower(), fieldType, page, count);
            return mapper.Map<List<CardDto>>(cardEntities);
        }

        [HttpPost(ApiRoutes.Cards.Post_StartCards)]
        public ActionResult<StartCardResponse> StartCards(short collectionId, [FromBody]CardsItem item)
        {
            var userId = HttpContext.GetUserId();

            if (userId.IsFailed)
                return BadRequest();

            var (closestRepeatInfo, error) = cardsService.Start(
                userId.Value,
                CollectionId.Create(collectionId).Value,
                UserId.Create(item.ScheduleUserId).Value,
                ScheduleId.Create(item.ScheduleId).Value, 
                item.CardIds);
            
            return closestRepeatInfo != null
                ? new StartCardResponse(
                    closestRepeatInfo.NextRepeatDate,
                    closestRepeatInfo.NextPhase == null 
                        ? null 
                        : mapper.Map<PhaseDto>(closestRepeatInfo.NextPhase),
                    closestRepeatInfo.NextPhaseIndex) 
                : BadRequest(error);
        }

        [HttpPatch(ApiRoutes.Cards.Path_RememberCard)]
        public async Task<ActionResult<RememberCardResponse>> RememberCard(short collectionId, [FromBody] RememberRequest request)
        {
            var userId = HttpContext.GetUserId();

            if (userId.IsFailed)
                return BadRequest();
            
            var (closestRepeatInfo, error) = await cardsService.Remember(
                userId.Value,
                CollectionId.Create(collectionId).Value,
                UserId.Create(request.ScheduleUserId).Value,
                ScheduleId.Create(request.ScheduleId).Value,
                request.PhaseIndex,
                ToCardServiceRememberItems(request.RememberItems)
            );

            return closestRepeatInfo != null
                ? new RememberCardResponse(
                    closestRepeatInfo.NextRepeatDate,
                    closestRepeatInfo.NextPhase == null
                        ? null
                        : mapper.Map<PhaseDto>(closestRepeatInfo.NextPhase),
                    closestRepeatInfo.NextPhaseIndex)
                : BadRequest(error);
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
