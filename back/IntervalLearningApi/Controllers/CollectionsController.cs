using DB.Models;
using DB.Models.Dictionary;
using DB.Models.Store;
using IntervalLearningApi.Extensions;
using IntervalLearningApi.Models.ByUser;
using IntervalLearningApi.Models.Dictionary;
using IntervalLearningApi.Services;
using Microsoft.AspNetCore.Authorization;
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
        
        [HttpGet("search")]
        public async Task<ActionResult<List<StoreCollection>>> SearchPublicCollection(short themeId, string? searchName = null, int page = 1, int count = 10)
        {
            var userId = HttpContext.GetUserId();
            var collections = await collectionService.SearchPublicCollections(userId, themeId, searchName ?? "", page, count);
            return collections.Select((t) => ToStoreCollection(t.collection, t.subscriber)).ToList();
        }

        [AllowAnonymous]
        [HttpGet("/public/{userId:long}-{collectionId}")]
        public async Task<ActionResult<Collection>> GetPublicCollection(long userId, short collectionId)
        {
            var collection = await collectionService.FindPublicCollection(userId, collectionId);
            return collection == null ? NotFound() : ToCollection(collection);
        }

        [HttpGet]
        public async Task<ActionResult<List<Collection>>> GetAll()
        {
            var userId = HttpContext.GetUserId();
            var collections = await collectionService.GetAllByUserId(userId);
            return collections.Select(ToCollection).ToList();
        }

        [HttpGet("words/random")]
        public async Task<ActionResult<GetRandomWordResponse>> GetRandomWords([FromQuery]short collectionId)
        {
            var userId = HttpContext.GetUserId();
            var (words, language, error) = await collectionService.GetRandomWords(userId, collectionId);

            return words == null || language == null
                ? BadRequest(error)
                : new GetRandomWordResponse(words, language);
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

        [HttpPost("{collectionId}/public")]
        public async Task<ActionResult<Collection>> MakePublic(short collectionId)
        {
            var userId = HttpContext.GetUserId();
            var (collection, error) = await collectionService.MakePublic(userId, collectionId).ConfigureAwait(false);
            return collection != null ? ToCollection(collection) : BadRequest(error);
        }

        [HttpPost("{collectionUserId}-{collectionId}/add/my-{myCollectionId}")]
        public async Task<ActionResult<Collection>> AddCardsToMyCollection(
            long collectionUserId,
            short collectionId,
            short myCollectionId,
            [FromQuery]bool checkUnique)
        {
            var userId = HttpContext.GetUserId();
            var (collection, error) = await collectionService.AddCardsToMyCollection(
                collectionUserId,
                collectionId,
                userId,
                myCollectionId,
                checkUnique);
            return collection != null ? ToCollection(collection) : BadRequest(error);
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
                c.NotStartedCardsCount,
                c.IsPublic,
                c.CollectionPublicationEntity == null ? null : ToCollectionPublication(c.CollectionPublicationEntity)
            );
        }

        public static StoreCollection ToStoreCollection(CollectionEntity c, PublicCollectionSubscriber? subscriber)
        {
            return new StoreCollection(
                c.ParentUser == null ? throw new InvalidOperationException() : ToUserInfo(c.ParentUser),
                c.ParentUserId,
                c.Id,
                c.Title,
                c.CreatedDate,
                c.ThemeId,
                c.CardsCount,
                c.NotStartedCardsCount,
                c.IsPublic,
                c.CollectionPublicationEntity == null
                    ? throw new InvalidOperationException()
                    : ToCollectionPublication(c.CollectionPublicationEntity),
                subscriber?.IsLiked ?? false,
                subscriber?.IsDisliked ?? false,
                subscriber?.IsAdded ?? false
            );
        }

        public static UserInfo ToUserInfo(UserEntity userEntity)
        {
            return new UserInfo(
                userEntity.Id,
                userEntity.FirstName,
                userEntity.LastName,
                userEntity.Email);
        }

        private static CollectionPublication ToCollectionPublication(CollectionPublicationEntity publication)
        {
            return new CollectionPublication(
                publication.PublishDate,
                publication.SubscribersCount,
                publication.LikesCount,
                publication.DislikesCount);
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

        public static WordDto ToWord(WordEntity word)
        {
            return new WordDto(
                word.Id,
                word.Word,
                word.Pronunciation,
                word.LanguageId);
        }

        public static LanguageDto ToLanguage(LanguageEntity language)
        {
            return new LanguageDto(
                language.Id,
                language.Name,
                language.NativeLanguageName,
                language.TranslationLinkTitle,
                language.TranslationLink);
        }

        public static TranslationDto ToTranslation(TranslationEntity arg)
        {
            return new TranslationDto(arg.LanguageId, arg.Id, arg.Translation);
        }
    }

    public class GetRandomWordResponse
    {
        public List<WordDto> Words { get; }

        public LanguageDto Language { get; }

        public GetRandomWordResponse(
            List<WordEntity> words, 
            LanguageEntity language)
        {
            Words = words.Select(CollectionsController.ToWord).ToList();
            Language = CollectionsController.ToLanguage(language);
        }
    }
}
