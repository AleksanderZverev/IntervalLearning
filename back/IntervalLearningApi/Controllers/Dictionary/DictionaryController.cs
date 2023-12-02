using Application.Commands.Dictionary.GetLanguages;
using Application.Commands.Dictionary.SearchWords;
using DB.Models.Dictionary.ValueObjects;
using IntervalLearningApi.Constants;
using IntervalLearningApi.Controllers.Dictionary.DTOs;
using IntervalLearningApi.Controllers.Dictionary.Requests.AddTranslations;
using IntervalLearningApi.Extensions;
using IntervalLearningApi.Infrastructure.CommandManager;
using IntervalLearningApi.Infrastructure.ValidatorResolver;
using IntervalLearningApi.Services.Dictionary;
using MapsterMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IntervalLearningApi.Controllers.Dictionary
{
    [Route(ApiRoutes.Dictionary.BasePath)]
    [Authorize]
    [ApiController]
    public class DictionaryController : ControllerBase
    {
        private readonly ValidatorResolver validatorResolver;
        private readonly IMapper mapper;
        private readonly CommandManager commandManager;
        private readonly DictionaryService dictionaryService;

        public DictionaryController(
            ValidatorResolver validatorResolver,
            IMapper mapper,
            CommandManager commandManager,
            DictionaryService dictionaryService)
        {
            this.validatorResolver = validatorResolver;
            this.mapper = mapper;
            this.commandManager = commandManager;
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

            var searchType = wordEmpty
                ? SearchWordType.Pronunciation
                : SearchWordType.Word;

            var textResult = WordText.Create(wordEmpty ? pronunciation : word);

            if (textResult.IsFailed)
                return textResult.ToErrorActionResult();

            var foundWordsResult = await commandManager
                .GetCommand<SearchWordsCommand>()
                .Handle(new SearchWordsRequest(textResult.Value, searchType, 30));

            return foundWordsResult.ToActionResult(foundWords => mapper.Map<List<WordDto>>(foundWords));
        }


        [HttpGet(ApiRoutes.Dictionary.Get_GenLanguages)]
        [AllowAnonymous]
        public async Task<ActionResult<List<LanguageDto>>> GetLanguages()
        {
            var languagesResult = await commandManager
                .GetCommand<GetLanguagesCommand>()
                .Handle(new GetLanguagesRequest());

            return languagesResult.ToActionResult(languages => mapper.Map<List<LanguageDto>>(languages));
        }

        [HttpPost(ApiRoutes.Dictionary.Post_AddTranslations)]
        public async Task<ActionResult<string>> AddTranslations(
            [FromBody] AddTranslationsRequest req)
        {
            var validation = validatorResolver.Validate(req);

            if (validation.IsFailed)
                return validation.ToErrorActionResult();
            
            var userId = HttpContext.GetUserId();

            if (userId.IsFailed)
                return BadRequest();

            var parseWordsResult = await dictionaryService.ParseWordsWithTranslations(
                userId.Value,
                req.LanguageId,
                req.TranslationLanguageId,
                req.Text);
            
            return parseWordsResult.ToActionResult();
        }

        [HttpGet(ApiRoutes.Dictionary.Get_GetTranslation)]
        public async Task<ActionResult<List<TranslationDto>>> GetTranslation(string word)
        {
            var userId = HttpContext.GetUserId();

            if (userId.IsFailed)
                return BadRequest();

            var translationsResult = await dictionaryService.GetTranslations(userId.Value, word);
            return translationsResult.ToActionResult(translations => mapper.Map<List<TranslationDto>>(translations));
        }
    }
}
