using IntervalLearningApi.Extensions;
using IntervalLearningApi.Models.Dictionary;
using IntervalLearningApi.Services.Dictionary;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace IntervalLearningApi.Controllers
{
    [Route("api/dictionary")]
    [ApiController]
    public class DictionaryController : ControllerBase
    {
        private readonly DictionaryService dictionaryService;

        public DictionaryController(DictionaryService dictionaryService)
        {
            this.dictionaryService = dictionaryService;
        }

        [HttpGet("translations")]
        public async Task<ActionResult<List<TranslationDto>>> GetTranslation(string word)
        {
            var userId = HttpContext.GetUserId();
            var (translations, error) = await dictionaryService.GetTranslations(userId, word);
            return translations == null
                ? BadRequest(error)
                : translations.Select(CollectionsController.ToTranslation).ToList();
        }
    }
}
