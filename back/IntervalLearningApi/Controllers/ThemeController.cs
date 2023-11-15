using System.ComponentModel.DataAnnotations;
using IntervalLearningApi.Constants;
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
        private readonly ThemeService themeService;

        public ThemeController(IMapper mapper, ThemeService themeService)
        {
            this.mapper = mapper;
            this.themeService = themeService;
        }

        [HttpGet(ApiRoutes.Themes.Get_GetAll)]
        public List<ThemeDto> GetAll()
        {
            var themes = themeService.GetAll();
            
            return themes is not { Count: > 0 }
                ? new List<ThemeDto>()
                : mapper.Map<List<ThemeDto>>(themes);
        }

        //[HttpPost]
        //public IActionResult CreateTheme([FromBody] CreateThemeItem themeItem)
        //{
        //    var (ok, error) = themeService.Create(themeItem.Name);
        //    return ok ? Ok() : BadRequest(error);
        //}
    }

    public class CreateThemeItem
    {
        [Required]
        [StringLength(100)]
        public string Name { get; set; }
    }
}
