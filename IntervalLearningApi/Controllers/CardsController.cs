using System.ComponentModel.DataAnnotations;
using IntervalLearningApi.Extensions;
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

        public CardsController(CardsService cardsService)
        {
            this.cardsService = cardsService;
        }

        [HttpPost]
        public IActionResult CreateCard(short collectionId, [FromBody]CreateCardItem item)
        {
            var userId = HttpContext.GetUserId();

            var (card, error) = cardsService.Create(
                userId,
                collectionId,
                item.FrontText,
                item.BackText,
                item.ScheduleId,
                item.Description,
                item.Examples);

            return card != null
                ? Ok(card)
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
        public string FrontText { get; set; }
        [Required]
        public string BackText { get; set; }
        public short ScheduleId { get; set; }
        public string? Description { get; set; }
        public List<string>? Examples { get; set; }
    }
}
