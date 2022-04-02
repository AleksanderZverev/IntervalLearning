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
            return Ok(cards.Select(CollectionsController.ToCard).ToList());
        }

        [HttpPost]
        public ActionResult<Card> CreateCard(short collectionId, [FromBody]CreateCardItem item)
        {
            if (item.Examples != null && item.Examples.Any(e => e.Length > 255))
            {
                return BadRequest();
            }

            var userId = HttpContext.GetUserId();

            var (card, error) = collectionService.AddCard(
                userId,
                collectionId,
                item.FrontText,
                item.BackText,
                item.ScheduleUserId,
                item.ScheduleId,
                item.Description,
                item.Examples);

            return card != null
                ? Ok(CollectionsController.ToCard(card))
                : BadRequest(error);
        }

        [HttpGet("{cardId}/start")]
        public IActionResult StartCard(short collectionId, short cardId)
        {
            var userId = HttpContext.GetUserId();
            var (ok, error) = cardsService.Start(userId, collectionId, cardId);
            return ok ? Ok() : BadRequest(error);
        }

        [HttpGet("{cardId}/finish")]
        public IActionResult FinishCard(short collectionId, short cardId)
        {
            var userId = HttpContext.GetUserId();
            var (ok, error) = cardsService.Finish(userId, collectionId, cardId);
            return ok ? Ok() : BadRequest(error);
        }

        [HttpGet("{cardId}/not-started")]
        public IActionResult SetNotStartedCard(short collectionId, short cardId)
        {
            var userId = HttpContext.GetUserId();
            var (ok, error) = cardsService.SetNotStarted(userId, collectionId, cardId);
            return ok ? Ok() : BadRequest(error);
        }

        [HttpPatch("{cardId}/remember")]
        public IActionResult RememberCard(short collectionId, short cardId, [FromBody] RememberItem rememberItem)
        {
            var userId = HttpContext.GetUserId();
            var (ok, error) = cardsService.Remember(
                userId,
                collectionId,
                cardId,
                rememberItem.Weight,
                rememberItem.PhaseStep,
                rememberItem.PassedSecondsFromLastStem
                );
            return ok ? Ok() : BadRequest(error);
        }
    }

    public class RememberItem
    {
        public float Weight { get; set; }
        public byte PhaseStep { get; set; }
        public int PassedSecondsFromLastStem { get; set; }
    }

    public class CreateCardItem
    {
        [Required]
        [StringLength(255)]
        public string FrontText { get; set; }
        [Required]
        [StringLength(255)]
        public string BackText { get; set; }
        [Required]
        public long ScheduleUserId { get; set; }
        [Required]
        public short ScheduleId { get; set; }
        [StringLength(500)]
        public string? Description { get; set; }
        [MaxLength(15)]
        public List<string>? Examples { get; set; }
    }
}
