using DB.Models.ValueObjects;
using Domain.Theme;
using FluentResults;

namespace Application.Common.Interfaces.Domain.Themes;

public interface IThemesQueryResolver
{
    Task<Theme?> Find(ThemeId themeId);
    Task<List<Theme>> GetAll();
    Task<List<Theme>> SearchByTitle(ThemeTitle title);
}