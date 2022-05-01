using System.ComponentModel.DataAnnotations;
using IntervalLearningApi.Extensions;
using IntervalLearningApi.Models.ByUser;
using IntervalLearningApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace IntervalLearningApi.Controllers
{
    [Route("api/collections/{collectionId}/cards")]
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

        public async Task<ActionResult<IList<Card>>> GetCards(short collectionId, [FromQuery] int page = 1, [FromQuery] int count = 10)
        {
            var userId = HttpContext.GetUserId();
            var cards = await cardsService.GetCards(userId, collectionId, page, count);
            return cards.Select(CollectionsController.ToCard).ToList();
        }

        [HttpGet("not-started")]
        public async Task<ActionResult<List<Card>>> GetNotStartedCards(
            short collectionId,
            long scheduleUserId,
            long scheduleId)
        {
            var userId = HttpContext.GetUserId();
            var (cards, error) = await cardsService.GetNotStartedCards(scheduleUserId, scheduleId, userId, collectionId);
            return cards == null ? BadRequest(error) : cards.Select(CollectionsController.ToCard).ToList();
        }

        [HttpPost]
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
                item.BackText,
                item.Description,
                item.Examples);

            return card != null
                ? CollectionsController.ToCard(card)
                : BadRequest(error);
        }

        [HttpPost("start")]
        public ActionResult<StartCardResponse> StartCards(short collectionId, [FromBody]CardsItem item)
        {
            var userId = HttpContext.GetUserId();
            var (closestRepeatDate, error) = cardsService.Start(userId, collectionId, item.ScheduleUserId, item.ScheduleId, item.CardIds);
            return string.IsNullOrEmpty(error) ? new StartCardResponse(closestRepeatDate) : BadRequest(error);
        }

        //[HttpGet("{cardId}/start")]
        //public IActionResult StartCard(short collectionId, short cardId)
        //{
        //    var userId = HttpContext.GetUserId();
        //    var (ok, error) = cardsService.Start(userId, collectionId, cardId);
        //    return ok ? Ok() : BadRequest(error);
        //}

        //[HttpGet("{cardId}/finish")]
        //public IActionResult FinishCard(short collectionId, short cardId)
        //{
        //    var userId = HttpContext.GetUserId();
        //    var (ok, error) = cardsService.Finish(userId, collectionId, cardId);
        //    return ok ? Ok() : BadRequest(error);
        //}

        //[HttpGet("{cardId}/not-started")]
        //public IActionResult SetNotStartedCard(short collectionId, short cardId)
        //{
        //    var userId = HttpContext.GetUserId();
        //    var (ok, error) = cardsService.SetNotStarted(userId, collectionId, cardId);
        //    return ok ? Ok() : BadRequest(error);
        //}

        [HttpPatch("remember")]
        public async Task<ActionResult<RememberCardResponse>> RememberCard(short collectionId, [FromBody] RememberRequest request)
        {
            var userId = HttpContext.GetUserId();

            var (ok, error, closestRepeatDate) = await cardsService.Remember(
                userId,
                collectionId,
                request.ScheduleUserId,
                request.ScheduleId,
                request.PhaseId,
                ToCardServiceRememberItems(request.RememberItems)
            );

            return ok ? new RememberCardResponse(closestRepeatDate) : BadRequest(error);
        }

        private List<CardsService.RememberItem> ToCardServiceRememberItems(List<RememberItem> requestRememberItems)
        {
            return requestRememberItems.Select(r => new CardsService.RememberItem(r.CardId, r.Weight)).ToList();
        }
    }

    public class StartCardResponse
    {
        public DateTime? NextRepeatDate { get; }

        public StartCardResponse(DateTime? nextRepeatDate)
        {
            NextRepeatDate = nextRepeatDate;
        }
    }

    public class RememberCardResponse
    {
        public DateTime? NextRepeatDate { get; }

        public RememberCardResponse(DateTime? nextRepeatDate)
        {
            NextRepeatDate = nextRepeatDate;
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
        public short PhaseId { get; set; }
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
        [Required]
        [StringLength(255)]
        public string BackText { get; set; }
        [StringLength(500)]
        public string? Description { get; set; }
        [MaxLength(15)]
        public List<string>? Examples { get; set; }
    }
}
