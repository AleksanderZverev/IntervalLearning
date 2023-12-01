using DB.Models.ValueObjects;
using Domain.Theme;

namespace Application.Common.Interfaces.DB.Queries.Study.Themes;

public interface IThemesQueryResolver
{
    Task<Theme?> Find(ThemeId themeId);
    Task<List<Theme>> GetAll();
    Task<List<Theme>> SearchByTitle(ThemeTitle title);
}