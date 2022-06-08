using System.ComponentModel.DataAnnotations;
using DB.Models.Store;
using IntervalLearningApi.Extensions;
using IntervalLearningApi.Models.ByUser;
using IntervalLearningApi.Models.Store;
using IntervalLearningApi.Services;
using IntervalLearningApi.Services.Store;
using Microsoft.AspNetCore.Mvc;

namespace IntervalLearningApi.Controllers
{
    [Route("api/collections/public")]
    [Authorize]
    [ApiController]
    public class PublicCollectionsController : ControllerBase
    {
        private readonly PublicCollectionService publicCollectionService;
        private readonly CollectionService collectionService;

        public PublicCollectionsController(PublicCollectionService publicCollectionService, CollectionService collectionService)
        {
            this.publicCollectionService = publicCollectionService;
            this.collectionService = collectionService;
        }

        [HttpPost]
        public ActionResult<PublicCollection> CreateCollection([FromBody] CreatePublicCollectionRequest item)
        {
            var userId = HttpContext.GetUserId();

            var (collection, error) = publicCollectionService.Create(new CreatePublicCollection(
                userId,
                item.Title,
                item.ShortDescription,
                item.ThemeId));

            return collection != null
                ? ToPublicCollection(collection)
                : BadRequest(error);
        }

        [HttpPatch("{collectionId}")]
        public ActionResult<PublicCollection> PatchCollection(short collectionId, [FromBody] CreatePublicCollectionRequest item)
        {
            var userId = HttpContext.GetUserId();

            var (collection, error) = publicCollectionService.Edit(new PatchPublicCollection(
                    item.Title,
                    item.ShortDescription,
                    item.ThemeId),
                userId,
                collectionId
            );

            return collection != null
                ? ToPublicCollection(collection)
                : BadRequest(error);
        }

        [HttpPost("{collectionUserId}-{collectionId}/my/{myCollectionId}")]
        public async Task<ActionResult<Collection>> AddCardsToMyCollection(
            long collectionUserId, 
            short collectionId, 
            short myCollectionId, 
            bool checkUnique)
        {
            var userId = HttpContext.GetUserId();
            var (collection, error) = await collectionService.AddCardsToMyCollection(
                collectionUserId,
                collectionId,
                userId,
                myCollectionId,
                checkUnique);
            return collection != null ? CollectionsController.ToCollection(collection) : BadRequest(error);
        }

        public static PublicCollection ToPublicCollection(PublicCollectionEntity collection)
            => new(
                collection.OwnerUserId,
                collection.Id,
                collection.Title,
                collection.ShortDescription,
                collection.ThemeId,
                collection.PublishDate,
                collection.CardsCount,
                collection.SubscribersCount,
                collection.LikesCount,
                collection.DislikesCount);

        public class CreatePublicCollectionRequest
        {
            [StringLength(150)]
            public string Title { get; set; }
            public short ThemeId { get; set; }
            [StringLength(500)]
            public string ShortDescription { get; set; }
        }
    }


}
