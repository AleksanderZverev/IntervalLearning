using System.ComponentModel.DataAnnotations;
using Application.Commands.Themes.CreateTheme;
using Application.Commands.Themes.GetThemes;
using DB.Models.ValueObjects;
using Infrastructure.Extensions;
using IntervalLearningApi.Constants;
using IntervalLearningApi.Extensions;
using IntervalLearningApi.Infrastructure.CommandManager;
using IntervalLearningApi.Models.ByUser;
using IntervalLearningApi.Services;
using MapsterMapper;
using Microsoft.AspNetCore.Mvc;

namespace IntervalLearningApi.Controllers
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

        // [HttpPost]
        // public IActionResult CreateTheme([FromBody] CreateThemeItem themeItem)
        // {
        //     var creationResult = commandManager
        //         .GetCommand<CreateThemeCommand>()
        //         .Handle(new CreateThemeRequest(ThemeTitle.Create(themeItem.Name).Value));
        //     
        //     return creationResult.IsCompletedSuccessfully ? Ok() : BadRequest();
        // }
    }

    public class CreateThemeItem
    {
        [Required]
        [StringLength(100)]
        public string Name { get; set; }
    }
}
