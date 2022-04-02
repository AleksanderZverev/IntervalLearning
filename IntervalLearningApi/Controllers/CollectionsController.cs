using DB.Models;
using IntervalLearningApi.Extensions;
using IntervalLearningApi.Models.ByUser;
using IntervalLearningApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace IntervalLearningApi.Controllers
{
    [Route("api/collections")]
    [Authorize]
    [ApiController]
    public class CollectionsController : ControllerBase
    {
        private readonly CollectionService collectionService;

        public CollectionsController(
            CollectionService collectionService)
        {
            this.collectionService = collectionService;
        }

        [HttpPost]
        public IActionResult CreateCollection([FromBody]CreateCollectionItem item)
        {
            var userId = HttpContext.GetUserId();

            var (collection, error) = collectionService.Create(
                userId,
                item.ScheduleUserId,
                item.ScheduleId,
                item.ThemeId,
                item.Title,
                item.IsDefaultBackSide);

            return collection != null
                ? Ok(ToCollection(collection))
                : BadRequest(error);
        }

        [HttpGet]
        public async Task<ActionResult<List<Collection>>> GetAll()
        {
            var userId = HttpContext.GetUserId();
            var collections = await collectionService.GetAllByUserId(userId);
            return Ok(collections.Select(ToCollection).ToList());
        }

        [HttpGet("{collectionId}")]
        public async Task<ActionResult<Collection>> Get(short collectionId)
        {
            var userId = HttpContext.GetUserId();
            var collection = await collectionService.Find(userId, collectionId).ConfigureAwait(false);
            return collection != null ? Ok(ToCollection(collection)) : NotFound();
        }

        public static Collection ToCollection(CollectionEntity c)
        {
            return new Collection(
                c.ParentUserId.ToString(),
                c.Id,
                c.Title,
                c.CreatedDate,
                c.DefaultRepeatsScheduleParentUserId,
                c.DefaultRepeatsScheduleId,
                c.ThemeId,
                c.CardsCount
            );
        }

        public static Card ToCard(CardEntity c)
        {
            return new Card(
                c.ParentUserId.ToString(),
                c.ParentCollectionId,
                c.Id,
                c.ParentRepeatsScheduleUserId,
                c.ParentRepeatsScheduleId,
                c.BackSideText,
                c.FrontSideText,
                c.CreatedDate,
                c.IsFinished,
                c.Description,
                c.Examples,
                c.Remembers.Select(ToRemember).ToList());
        }

        private static Remember ToRemember(RememberEntity r)
        {
            return new Remember(
                r.ParentUserId,
                r.ParentCollectionId,
                r.ParentCardId,
                r.Id,
                r.Weight,
                r.PhaseStep,
                r.PassedSecondsFromLastStep);
        }
    }
}
