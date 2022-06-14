using DB.Models.Dictionary;
using IntervalLearningApi.Extensions;
using IntervalLearningApi.Models.Dictionary;
using IntervalLearningApi.Services.Dictionary;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace IntervalLearningApi.Controllers
{
    [Route("api/dictionary")]
    [Authorize]
    [ApiController]
    public class DictionaryController : ControllerBase
    {
        private readonly DictionaryService dictionaryService;

        public DictionaryController(DictionaryService dictionaryService)
        {
            this.dictionaryService = dictionaryService;
        }

        [HttpGet("words/search")]
        public async Task<ActionResult<List<WordDto>>> SearchWords(
            [FromQuery] string? word = null,
            [FromQuery] string? pronunciation = null
            )
        {
            var wordEmpty = string.IsNullOrEmpty(word);
            var pronunciationEmpty = string.IsNullOrEmpty(pronunciation);

            if (wordEmpty && pronunciationEmpty)
            {
                return BadRequest();
            }

            List<WordEntity>? foundWords = null;

            if (pronunciationEmpty)
            {
                foundWords = await dictionaryService.FindWord(word);
            }

            if (wordEmpty)
            {
                foundWords = await dictionaryService.FindWordByPronunciation(pronunciation);
            }

            return foundWords?.Select(CollectionsController.ToWord).ToList() ?? new List<WordDto>();
        }


        [HttpGet("languages")]
        [AllowAnonymous]
        public async Task<ActionResult<List<LanguageDto>>> GetLanguages()
        {
            var languages = await dictionaryService.GetLanguages();
            return languages.Select(CollectionsController.ToLanguage).ToList();
        }

        [HttpPost("translations")]
        public async Task<ActionResult<string>> AddTranslations([FromBody] AddTranslationsRequest req)
        {
            var userId = HttpContext.GetUserId();
            var (ok, error) = await dictionaryService.ParseWordsWithTranslations(userId, req.LanguageId, req.TranslationLanguageId, req.Text);
            return ok != null ? ok : BadRequest(error);
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

        public class AddTranslationsRequest
        {
            public short LanguageId { get; set; }
            public short TranslationLanguageId { get; set; }
            public string Text { get; set; }
        }
    }
}
