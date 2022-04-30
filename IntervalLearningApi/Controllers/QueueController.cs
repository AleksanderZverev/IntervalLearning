using IntervalLearningApi.Extensions;
using IntervalLearningApi.Models.ByUser;
using IntervalLearningApi.Services;
using Microsoft.AspNetCore.Mvc;
using NodaTime;

namespace IntervalLearningApi.Controllers
{
    [Route("api/queue")]
    [Authorize]
    [ApiController]
    public class QueueController : ControllerBase
    {

        private readonly CardsService cardsService;
        private readonly CollectionService collectionService;

        public QueueController(CardsService cardsService, CollectionService collectionService)
        {
            this.cardsService = cardsService;
            this.collectionService = collectionService;
        }

        [HttpGet("learn")]
        public async Task<ActionResult<IList<Collection>>> GetCardsFromQueue()
        {
            var userId = HttpContext.GetUserId();
            var (collections, cardsWithDates) = await cardsService.GetLearningCollectionWithCards(userId);

            var collectionsDto = collections.Select(CollectionsController.ToCollection).ToList();
            var cardsDto = cardsWithDates
                .Select(tuple => new QueueCard(tuple.Item1, CollectionsController.ToCard(tuple.Item2)))
                .ToList();

            var response = new LearnResponse(collectionsDto, cardsDto);
            return Ok(response);
        }

        [HttpGet("{collectionId}/cards/repeat")]
        public async Task<ActionResult<List<Card>>> GetCardsQueue(short collectionId, [FromQuery] DateTime date)
        {
            var userId = HttpContext.GetUserId();
            var cards = await cardsService.GetCardsQueue(userId, collectionId, date);
            return cards.Select(CollectionsController.ToCard).ToList();
        }
    }

    public class LearnResponse
    {
        public List<Collection> Collections { get; }

        public List<QueueCard> QueueCards { get; }

        public LearnResponse(List<Collection> collections, List<QueueCard> queueCards)
        {
            Collections = collections;
            QueueCards = queueCards;
        }
    }

    public class QueueCard
    {
        public DateTime RepeatDate { get; }

        public Card Card { get; }

        public QueueCard(DateTime repeatDate, Card card)
        {
            RepeatDate = repeatDate;
            Card = card;
        }
    }
}
