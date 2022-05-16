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
        public ActionResult<Collection> CreateCollection([FromBody]CreateCollectionItem item)
        {
            var userId = HttpContext.GetUserId();

            var (collection, error) = collectionService.CreateOrEdit(
                new CollectionService.CreateOrPatchCollection(
                    userId,
                    item.Title,
                    item.IsDefaultBackSide,
                    item.ThemeId
                ),
                item.CollectionId);

            return collection != null
                ? ToCollection(collection)
                : BadRequest(error);
        }

        [HttpGet]
        public async Task<ActionResult<List<Collection>>> GetAll()
        {
            var userId = HttpContext.GetUserId();
            var collections = await collectionService.GetAllByUserId(userId);
            return collections.Select(ToCollection).ToList();
        }
        
        [HttpGet("repeat")]
        public async Task<ActionResult<RepeatingCollectionResponse>> GetRepeatCollections()
        {
            var userId = HttpContext.GetUserId();
            var dateToRepeatingCollections = await collectionService.GetRepeatCollections(userId);

            return new RepeatingCollectionResponse(
                dateToRepeatingCollections
                    .ToDictionary(
                        p => p.Key,
                        p => p.Value
                            .Select(c => new RepeatingPhaseDto(
                                c.ScheduleUserId,
                                c.ScheduleId,
                                c.PhaseIndex,
                                c.SecondsFromLastPhase,
                                c.Description,
                                c.RepeatingCollections
                                    .Select(r => new RepeatingCollectionDto(
                                        ToCollection(r.Collection), 
                                        r.CardsToRepeatCount))
                                    .ToList()))
                            .ToList()));
        }

        [HttpGet("not-finished")]
        public async Task<ActionResult<GetNotFinishedResponse>> GetNotFinished(
            long scheduleUserId,
            short scheduleId,
            int page = 1, 
            int count = 30)
        {
            var userId = HttpContext.GetUserId();
            var (totalCollections, canStartCollections) = await collectionService.GetCanStart(userId, scheduleUserId, scheduleId, page, count);
            return new GetNotFinishedResponse(totalCollections, ToCollection(canStartCollections));
        }

        [HttpGet("{collectionId}")]
        public async Task<ActionResult<Collection>> Get(short collectionId)
        {
            var userId = HttpContext.GetUserId();
            var collection = await collectionService.Find(userId, collectionId).ConfigureAwait(false);
            return collection != null ? Ok(ToCollection(collection)) : NotFound();
        }

        public static List<Collection> ToCollection(IEnumerable<CollectionEntity> collections)
            => collections.Select(ToCollection).ToList();

        public static Collection ToCollection(CollectionEntity c)
        {
            return new Collection(
                c.ParentUserId,
                c.Id,
                c.Title,
                c.CreatedDate,
                c.ThemeId,
                c.CardsCount,
                c.NotStartedCardsCount
            );
        }

        public static Card ToCard(CardEntity c)
        {
            return new Card(
                c.ParentUserId,
                c.ParentCollectionId,
                c.Id,
                c.BackSideText,
                c.PromptText,
                c.FrontSideText,
                c.CreatedDate,
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
                r.PhaseIndex,
                r.RepeatedDate);
        }
    }
}
