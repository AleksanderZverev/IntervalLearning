using System.ComponentModel.DataAnnotations;
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
        public async Task<ActionResult<Card>> GetCard(short collectionId, short cardId)
        {
            var userId = HttpContext.GetUserId();

            if (userId.IsFailed)
                return BadRequest();

            var card = await cardsService.FindCard(userId.Value, CollectionId.Create(collectionId).Value, cardId);
            return card == null 
                ? NotFound()
                : mapper.Map<Card>(card);
        }

        [HttpGet(ApiRoutes.Cards.Get_GetAll)]
        public async Task<ActionResult<IList<Card>>> GetCards(short collectionId, [FromQuery] int page = 1, [FromQuery] int count = 10)
        {
            var userId = HttpContext.GetUserId();

            if (userId.IsFailed)
                return BadRequest();


            var cards = await cardsService.GetCards(userId.Value, CollectionId.Create(collectionId).Value, page, count);
            return mapper.Map<List<Card>>(cards);
        }

        [HttpGet(ApiRoutes.Cards.Get_GetCardQueue)]
        public async Task<ActionResult<List<Card>>> GetCardsQueue(
            short collectionId,
            [FromQuery] long scheduleUserId,
            [FromQuery] short scheduleId,
            [FromQuery] short phaseIndex,
            [FromQuery] DateTime date)
        {
            var userId = HttpContext.GetUserId();

            if (userId.IsFailed)
                return BadRequest();

            var cards = await cardsService.GetCardsQueue(userId.Value, CollectionId.Create(collectionId).Value, UserId.Create(scheduleUserId).Value, scheduleId, phaseIndex, date);
            return mapper.Map<List<Card>>(cards);
        }

        [HttpGet(ApiRoutes.Cards.Get_GetNotStartedCards)]
        public async Task<ActionResult<List<Card>>> GetNotStartedCards(
            short collectionId,
            long scheduleUserId,
            short scheduleId,
            [Range(1, 1000)]int count)
        {
            var userId = HttpContext.GetUserId();

            if (userId.IsFailed)
                return BadRequest();
            
            var (cards, error) = await cardsService.GetNotStartedCards(UserId.Create(scheduleUserId).Value, scheduleId, userId.Value, CollectionId.Create(collectionId).Value, count);
            return cards == null 
                ? BadRequest(error) 
                : mapper.Map<List<Card>>(cards);
        }

        [HttpPost(ApiRoutes.Cards.Post_CreateCard)]
        public ActionResult<Card> CreateCard(short collectionId, [FromBody]CreateCardItem item)
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
                item.CardId,
                item.FrontText,
                item.PromptText ?? string.Empty,
                item.BackText,
                item.Description,
                item.Examples);

            return card != null
                ? mapper.Map<Card>(card)
                : BadRequest(error);
        }

        [HttpDelete(ApiRoutes.Cards.Delete_DeleteCard)]
        public async Task<ActionResult<Card>> DeleteCard(short collectionId, short cardId)
        {
            var userId = HttpContext.GetUserId();

            if (userId.IsFailed)
                return BadRequest();

            var (cardEntity, error) = await collectionService.DeleteCard(userId.Value, CollectionId.Create(collectionId).Value, cardId);
            return cardEntity != null 
                ? mapper.Map<Card>(cardEntity)
                : BadRequest(error);
        }

        [HttpPost(ApiRoutes.Cards.Post_MoveCard)]
        public async Task<ActionResult<Card>> MoveCard(short collectionId, [FromBody] MoveRequest request)
        {
            var userId = HttpContext.GetUserId();

            if (userId.IsFailed)
                return BadRequest();

            var (cardEntity, error) = await collectionService.MoveCard(userId.Value, CollectionId.Create(collectionId).Value, CollectionId.Create(request.DestinationCollectionId).Value, request.CardId);
            return cardEntity != null 
                ? mapper.Map<Card>(cardEntity) 
                : BadRequest(error);
        }

        [HttpGet(ApiRoutes.Cards.Get_SearchCard)]
        public async Task<ActionResult<List<Card>>> SearchCard(
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
            return mapper.Map<List<Card>>(cardEntities);
        }

        [HttpPost(ApiRoutes.Cards.Post_StartCards)]
        public ActionResult<StartCardResponse> StartCards(short collectionId, [FromBody]CardsItem item)
        {
            var userId = HttpContext.GetUserId();

            if (userId.IsFailed)
                return BadRequest();

            var (closestRepeatInfo, error) = cardsService.Start(userId.Value, CollectionId.Create(collectionId).Value, UserId.Create(item.ScheduleUserId).Value, item.ScheduleId, item.CardIds);
            return closestRepeatInfo != null
                ? new StartCardResponse(
                    closestRepeatInfo.NextRepeatDate,
                    closestRepeatInfo.NextPhase == null 
                        ? null 
                        : mapper.Map<Phase>(closestRepeatInfo.NextPhase),
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
                request.ScheduleId,
                request.PhaseIndex,
                ToCardServiceRememberItems(request.RememberItems)
            );

            return closestRepeatInfo != null
                ? new RememberCardResponse(
                    closestRepeatInfo.NextRepeatDate,
                    closestRepeatInfo.NextPhase == null
                        ? null
                        : mapper.Map<Phase>(closestRepeatInfo.NextPhase),
                    closestRepeatInfo.NextPhaseIndex)
                : BadRequest(error);
        }

        private List<CardsService.RememberItem> ToCardServiceRememberItems(List<RememberItem> requestRememberItems)
        {
            return requestRememberItems.Select(r => new CardsService.RememberItem(r.CardId, r.Weight)).ToList();
        }
    }

    public class StartCardResponse
    {
        public DateTime? NextRepeatDate { get; }
        public Phase? NextRepeatPhase { get; }
        public int NextPhaseIndex { get; }

        public StartCardResponse(DateTime? nextRepeatDate, Phase? nextRepeatPhase, int nextPhaseIndex)
        {
            NextRepeatDate = nextRepeatDate;
            NextRepeatPhase = nextRepeatPhase;
            NextPhaseIndex = nextPhaseIndex;
        }
    }

    public class RememberCardResponse
    {
        public DateTime? NextRepeatDate { get; }
        public Phase? NextRepeatPhase { get; }
        public int NextPhaseIndex { get; }

        public RememberCardResponse(DateTime? nextRepeatDate, Phase? nextRepeatPhase, int nextPhaseIndex)
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
        public List<RememberItem> RememberItems { get; set; }
        public long ScheduleUserId { get; set; }
        public short ScheduleId { get; set; }
        public short PhaseIndex { get; set; }
    }

    public class MoveRequest
    {
        public short DestinationCollectionId { get; set; }
        public short CardId { get; set; }
    }

    public class RememberItem
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
