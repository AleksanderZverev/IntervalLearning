using System.ComponentModel.DataAnnotations;
using Application.Commands.Themes.CreateTheme;
using Application.Commands.Themes.DeleteTheme;
using Application.Commands.Themes.GetThemes;
using Application.Commands.Themes.UpdateTheme;
using Domain.Theme.ValueObjects;
using IntervalLearningApi.Constants;
using IntervalLearningApi.Controllers.Study.Themes.DTOs;
using IntervalLearningApi.Extensions;
using IntervalLearningApi.Infrastructure.CommandManager;
using MapsterMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IntervalLearningApi.Controllers.Study.Themes
{
    [Route(ApiRoutes.Themes.BasePath)]
    [ApiController]
    public class ThemeController : ControllerBase
    {
        private readonly IMapper mapper;
        private readonly CommandManager commandManager;

        public ThemeController(
            IMapper mapper,
            CommandManager commandManager)
        {
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
        public async Task<ActionResult> Create([FromBody] ThemeRequest request)
        {
            var titleResult = ThemeTitle.Create(request.Name);
            if (titleResult.IsFailed)
                return BadRequest(titleResult.Errors);

            var result = await commandManager
                .GetCommand<CreateThemeCommand>()
                .Handle(new CreateThemeRequest(titleResult.Value));

            return result.ToActionResult();
        }

        [HttpPut(ApiRoutes.Themes.Put_Update)]
        [Authorize]
        public async Task<ActionResult<ThemeDto>> Update([FromRoute] short themeId, [FromBody] ThemeRequest request)
        {
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

    public class ThemeRequest
    {
        [Required]
        [StringLength(100)]
        public string Name { get; set; }
    }
}
