using Application.Commands.Themes.CreateTheme;
using Application.Commands.Themes.DeleteTheme;
using Application.Commands.Themes.GetThemes;
using Application.Commands.Themes.UpdateTheme;
using Domain.Theme.ValueObjects;
using IntervalLearningApi.Constants;
using IntervalLearningApi.Controllers.Study.Themes.DTOs;
using IntervalLearningApi.Controllers.Study.Themes.Requests;
using IntervalLearningApi.Extensions;
using IntervalLearningApi.Infrastructure.CommandManager;
using IntervalLearningApi.Infrastructure.ValidatorResolver;
using MapsterMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IntervalLearningApi.Controllers.Study.Themes
{
    [Route(ApiRoutes.Themes.BasePath)]
    [ApiController]
    public class ThemeController : ControllerBase
    {
        private readonly ValidatorResolver validatorResolver;
        private readonly IMapper mapper;
        private readonly CommandManager commandManager;

        public ThemeController(
            ValidatorResolver validatorResolver,
            IMapper mapper,
            CommandManager commandManager)
        {
            this.validatorResolver = validatorResolver;
            this.mapper = mapper;
            this.commandManager = commandManager;
        }

        [HttpGet(ApiRoutes.Themes.Get_GetAll)]
        public async Task<ActionResult<List<ThemeDto>>> GetAll()
        {
            var themesResult = await commandManager
                .GetCommand<GetThemesCommand>()
                .Handle(new GetThemesRequest());

            return themesResult.ToActionResult(themes => mapper.Map<List<ThemeDto>>(themes));
        }

        [HttpPost(ApiRoutes.Themes.Post_Create)]
        [Authorize]
        public async Task<ActionResult<ThemeDto>> Create([FromBody] ThemeRequest request)
        {
            var validation = validatorResolver.Validate(request);
            if (validation.IsFailed)
                return validation.ToErrorActionResult();

            var titleResult = ThemeTitle.Create(request.Name);
            if (titleResult.IsFailed)
                return BadRequest(titleResult.Errors);

            var result = await commandManager
                .GetCommand<CreateThemeCommand>()
                .Handle(new CreateThemeRequest(titleResult.Value));

            return result.ToActionResult(theme => mapper.Map<ThemeDto>(theme));
        }

        [HttpPut(ApiRoutes.Themes.Put_Update)]
        [Authorize]
        public async Task<ActionResult<ThemeDto>> Update([FromRoute] short themeId, [FromBody] ThemeRequest request)
        {
            var validation = validatorResolver.Validate(request);
            if (validation.IsFailed)
                return validation.ToErrorActionResult();

            var idResult = ThemeId.Create(themeId);
            if (idResult.IsFailed)
                return BadRequest();

            var titleResult = ThemeTitle.Create(request.Name);
            if (titleResult.IsFailed)
                return BadRequest(titleResult.Errors);

            var result = await commandManager
                .GetCommand<UpdateThemeCommand>()
                .Handle(new UpdateThemeRequest(idResult.Value, titleResult.Value));

            return result.ToActionResult(theme => mapper.Map<ThemeDto>(theme));
        }

        [HttpDelete(ApiRoutes.Themes.Delete_Delete)]
        [Authorize]
        public async Task<ActionResult> Delete([FromRoute] short themeId)
        {
            var idResult = ThemeId.Create(themeId);
            if (idResult.IsFailed)
                return BadRequest();

            var result = await commandManager
                .GetCommand<DeleteThemeCommand>()
                .Handle(new DeleteThemeRequest(idResult.Value));

            return result.ToActionResult();
        }
    }
}
