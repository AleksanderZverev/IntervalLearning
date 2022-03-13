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
        public IActionResult CreateCollection(
            short repeatsScheduleId,
            short themeId,
            string title,
            bool isDefaultBackSide)
        {
            var userId = HttpContext.GetUserId();

            var (collection, error) = collectionService.Create(
                userId,
                repeatsScheduleId,
                themeId,
                title,
                isDefaultBackSide);

            return collection != null
                ? Ok(collection)
                : BadRequest(error);
        }

        [HttpGet]
        public List<Collection> GetAll()
        {
            var userId = HttpContext.GetUserId();
            var collections = collectionService.GetAllByUserId(userId).Select(ToCollection).ToList();
            return collections;
        }

        private static Collection ToCollection(CollectionEntity c)
        {
            return new Collection(
                c.ParentUserId.ToString(),
                c.Id,
                c.Title,
                c.CreatedDate,
                c.DefaultRepeatsScheduleId,
                c.ThemeId,
                c.Cards.Select(ToCard).ToList()
            );
        }

        private static Card ToCard(CardEntity c)
        {
            return new Card(
                c.ParentUserId.ToString(),
                c.ParentCollectionId,
                c.Id,
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
