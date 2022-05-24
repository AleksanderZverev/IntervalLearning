using System.ComponentModel.DataAnnotations;
using IntervalLearningApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace IntervalLearningApi.Controllers
{
    [Route("api/themes")]
    [ApiController]
    public class ThemeController : ControllerBase
    {
        private readonly ThemeService themeService;

        public ThemeController(ThemeService themeService)
        {
            this.themeService = themeService;
        }

        [HttpGet]
        public List<Theme> GetAll()
        {
            return themeService.GetAll().Select(t => new Theme(t.Id, t.Name, t.LanguageId)).ToList();
        }

        //[HttpPost]
        //public IActionResult CreateTheme([FromBody] CreateThemeItem themeItem)
        //{
        //    var (ok, error) = themeService.Create(themeItem.Name);
        //    return ok ? Ok() : BadRequest(error);
        //}
    }

    public class Theme
    {
        public short Id { get; }
        public string Name { get; }
        public string? LanguageId { get; }

        public Theme(short id, string name, short? languageId)
        {
            Id = id;
            Name = name;
            LanguageId = languageId?.ToString();
        }
    }

    public class CreateThemeItem
    {
        [Required]
        [StringLength(100)]
        public string Name { get; set; }
    }
}
