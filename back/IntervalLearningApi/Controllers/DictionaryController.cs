using DB.Models.Dictionary;
using IntervalLearningApi.Constants;
using IntervalLearningApi.Extensions;
using IntervalLearningApi.Models.Dictionary;
using IntervalLearningApi.Services.Dictionary;
using MapsterMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IntervalLearningApi.Controllers
{
    [Route(ApiRoutes.Dictionary.BasePath)]
    [Authorize]
    [ApiController]
    public class DictionaryController : ControllerBase
    {
        private readonly IMapper mapper;
        private readonly DictionaryService dictionaryService;

        public DictionaryController(IMapper mapper, DictionaryService dictionaryService)
        {
            this.mapper = mapper;
            this.dictionaryService = dictionaryService;
        }

        [HttpGet(ApiRoutes.Dictionary.Get_SearchWords)]
        public async Task<ActionResult<List<WordDto>>> SearchWords(
            [FromQuery] string? word = null,
            [FromQuery] string? pronunciation = null)
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
            
            return foundWords is not { Count: > 0 }
                ? new List<WordDto>()
                : mapper.Map<List<WordDto>>(foundWords);
        }


        [HttpGet(ApiRoutes.Dictionary.Get_GenLanguages)]
        [AllowAnonymous]
        public async Task<ActionResult<List<LanguageDto>>> GetLanguages()
        {
            var languages = await dictionaryService.GetLanguages();
            return mapper.Map<List<LanguageDto>>(languages);
        }

        [HttpPost(ApiRoutes.Dictionary.Post_AddTranslations)]
        public async Task<ActionResult<string>> AddTranslations([FromBody] AddTranslationsRequest req)
        {
            var userId = HttpContext.GetUserId();

            if (userId.IsFailed)
                return BadRequest();

            var (ok, error) = await dictionaryService.ParseWordsWithTranslations(userId.Value, req.LanguageId, req.TranslationLanguageId, req.Text);
            return ok != null ? ok : BadRequest(error);
        }

        [HttpGet(ApiRoutes.Dictionary.Get_GetTranslation)]
        public async Task<ActionResult<List<TranslationDto>>> GetTranslation(string word)
        {
            var userId = HttpContext.GetUserId();

            if (userId.IsFailed)
                return BadRequest();

            var (translations, error) = await dictionaryService.GetTranslations(userId.Value, word);
            return translations == null
                ? BadRequest(error)
                : mapper.Map<List<TranslationDto>>(translations);
        }

        public class AddTranslationsRequest
        {
            public short LanguageId { get; set; }
            public short TranslationLanguageId { get; set; }
            public string Text { get; set; }
        }
    }
}
