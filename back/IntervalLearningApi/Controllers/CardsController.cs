using System.ComponentModel.DataAnnotations;
using IntervalLearningApi.Constants;
using IntervalLearningApi.Extensions;
using IntervalLearningApi.Models;
using IntervalLearningApi.Models.ByUser;
using IntervalLearningApi.Models.RepeatsSchedule;
using IntervalLearningApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace IntervalLearningApi.Controllers
{
    [Route(ApiRoutes.Cards.BasePath)]
    [Authorize]
    [ApiController]
    public class CardsController : ControllerBase
    {
        private readonly CardsService cardsService;
        private readonly CollectionService collectionService;

        public CardsController(CardsService cardsService, CollectionService collectionService)
        {
            this.cardsService = cardsService;
            this.collectionService = collectionService;
        }

        [HttpGet(ApiRoutes.Cards.Get_Card)]
        public async Task<ActionResult<Card>> GetCard(short collectionId, short cardId)
        {
            var userId = HttpContext.GetUserId();
            var card = await cardsService.FindCard(userId, collectionId, cardId);
            return card == null 
                ? NotFound() 
                : CollectionsController.ToCard(card);
        }

        [HttpGet(ApiRoutes.Cards.Get_GetAll)]
        public async Task<ActionResult<IList<Card>>> GetCards(short collectionId, [FromQuery] int page = 1, [FromQuery] int count = 10)
        {
            var userId = HttpContext.GetUserId();
            var cards = await cardsService.GetCards(userId, collectionId, page, count);
            return cards.Select(CollectionsController.ToCard).ToList();
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
            var cards = await cardsService.GetCardsQueue(userId, collectionId, scheduleUserId, scheduleId, phaseIndex, date);
            return cards.Select(CollectionsController.ToCard).ToList();
        }

        [HttpGet(ApiRoutes.Cards.Get_GetNotStartedCards)]
        public async Task<ActionResult<List<Card>>> GetNotStartedCards(
            short collectionId,
            long scheduleUserId,
            short scheduleId,
            [Range(1, 1000)]int count)
        {
            var userId = HttpContext.GetUserId();
            var (cards, error) = await cardsService.GetNotStartedCards(scheduleUserId, scheduleId, userId, collectionId, count);
            return cards == null ? BadRequest(error) : cards.Select(CollectionsController.ToCard).ToList();
        }

        [HttpPost(ApiRoutes.Cards.Post_CreateCard)]
        public ActionResult<Card> CreateCard(short collectionId, [FromBody]CreateCardItem item)
        {
            if (item.Examples != null && item.Examples.Any(e => e.Length > 255))
            {
                return BadRequest();
            }

            var userId = HttpContext.GetUserId();

            var (card, error) = collectionService.CreateOrEditCard(
                userId,
                collectionId,
                item.CardId,
                item.FrontText,
                item.PromptText ?? string.Empty,
                item.BackText,
                item.Description,
                item.Examples);

            return card != null
                ? CollectionsController.ToCard(card)
                : BadRequest(error);
        }

        [HttpDelete(ApiRoutes.Cards.Delete_DeleteCard)]
        public async Task<ActionResult<Card>> DeleteCard(short collectionId, short cardId)
        {
            var userId = HttpContext.GetUserId();
            var (cardEntity, error) = await collectionService.DeleteCard(userId, collectionId, cardId);
            return cardEntity != null ? CollectionsController.ToCard(cardEntity) : BadRequest(error);
        }

        [HttpPost(ApiRoutes.Cards.Post_MoveCard)]
        public async Task<ActionResult<Card>> MoveCard(short collectionId, [FromBody] MoveRequest request)
        {
            var userId = HttpContext.GetUserId();
            var (cardEntity, error) = await collectionService.MoveCard(userId, collectionId, request.DestinationCollectionId, request.CardId);
            return cardEntity != null ? CollectionsController.ToCard(cardEntity) : BadRequest(error);
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
            var cardEntities = await cardsService.Search(userId, collectionId, searchValue.ToLower(), fieldType, page, count);
            return cardEntities.Select(CollectionsController.ToCard).ToList();
        }

        [HttpPost(ApiRoutes.Cards.Post_StartCards)]
        public ActionResult<StartCardResponse> StartCards(short collectionId, [FromBody]CardsItem item)
        {
            var userId = HttpContext.GetUserId();
            var (closestRepeatInfo, error) = cardsService.Start(userId, collectionId, item.ScheduleUserId, item.ScheduleId, item.CardIds);
            return closestRepeatInfo != null
                ? new StartCardResponse(
                    closestRepeatInfo.NextRepeatDate,
                    closestRepeatInfo.NextPhase == null 
                        ? null 
                        : RepeatsScheduleController.ToPhase(closestRepeatInfo.NextPhase),
                    closestRepeatInfo.NextPhaseIndex) 
                : BadRequest(error);
        }

        [HttpPatch(ApiRoutes.Cards.Path_RememberCard)]
        public async Task<ActionResult<RememberCardResponse>> RememberCard(short collectionId, [FromBody] RememberRequest request)
        {
            var userId = HttpContext.GetUserId();

            var (closestRepeatInfo, error) = await cardsService.Remember(
                userId,
                collectionId,
                request.ScheduleUserId,
                request.ScheduleId,
                request.PhaseIndex,
                ToCardServiceRememberItems(request.RememberItems)
            );

            return closestRepeatInfo != null
                ? new RememberCardResponse(
                    closestRepeatInfo.NextRepeatDate,
                    closestRepeatInfo.NextPhase == null
                        ? null
                        : RepeatsScheduleController.ToPhase(closestRepeatInfo.NextPhase),
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
