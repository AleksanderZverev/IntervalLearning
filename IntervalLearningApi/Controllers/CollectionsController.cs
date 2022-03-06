using DB;
using DB.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;

namespace IntervalLearningApi.Controllers
{
    [Route("api/collections")]
    [ApiController]
    public class CollectionsController : ControllerBase
    {
        public CollectionsController()
        {

        }

        //[HttpPost]
        //public IActionResult CreateCollections()
        //{

        //}
    }


    public class CollectionService
    {
        private readonly ApplicationContext db;
        private readonly CardsService cardsService;

        public CollectionService(ApplicationContext db, CardsService cardsService)
        {
            this.db = db;
            this.cardsService = cardsService;
        }

        public List<CollectionEntity> GetAllByUserId(long userId)
        {
            var collections = db.Collections
                .Where(c => c.ParentUserId == userId)
                .Include(c => c.Cards)
                .ThenInclude(c => c.Remembers)
                .AsNoTracking()
                .ToList();

            return collections;
        }

        public CollectionEntity Create(
            long userId, 
            short repeatsScheduleId, 
            short themeId, 
            string title, 
            bool isDefaultBackSide)
        {
            var collection = new CollectionEntity(
                userId,
                repeatsScheduleId,
                themeId,
                title,
                isDefaultBackSide
            );

            db.Collections.Add(collection);
            db.SaveChanges();
            return collection;
        }

        public CollectionEntity? AddCard(
            long userId,
            short collectionId,
            string frontText,
            string backText,
            short scheduleId,
            string description = null,
            List<string> examples = null)
        {
            var collection = db.Collections.Find(userId, collectionId);

            if (collection == null)
                return null;

            var card = cardsService.Create(
                userId, collectionId, frontText, backText, scheduleId, description, examples);

            collection.Cards.Add(card);
            return collection;
        }
    }

    public class CardsService
    {
        private readonly ApplicationContext db;

        public CardsService(ApplicationContext db)
        {
            this.db = db;
        }

        public CardEntity Create(
            long userId,
            short collectionId,
            string frontText,
            string backText,
            short scheduleId,
            string description = null,
            List<string> examples = null)
        {
            var card = new CardEntity(
                userId,
                collectionId,
                frontText,
                backText,
                scheduleId,
                description,
                examples
            );

            db.Entry(card).State = EntityState.Added;
            db.SaveChanges();
            return card;
        }

        public bool Repeated(
            long userId,
            short collectionId,
            byte cardId,
            float weight,
            byte phaseStep, 
            int passedSecondsFromLastStep)
        {
            var card = db.Cards.Find(userId, collectionId, cardId);

            if (card == null)
                return false;

            var rememberItem = new RememberEntity(
                userId,
                collectionId,
                cardId,
                weight,
                phaseStep,
                passedSecondsFromLastStep);

            db.Entry(rememberItem).State = EntityState.Added;
            db.SaveChanges();

            return true;
        }
    }
}
