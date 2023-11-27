using DB.Models.ValueObjects;
using Domain.Theme;

namespace Application.Common.Interfaces.Domain.Themes;

public interface IThemesQueryResolver
{
    Task<Theme?> Find(ThemeId themeId);
}