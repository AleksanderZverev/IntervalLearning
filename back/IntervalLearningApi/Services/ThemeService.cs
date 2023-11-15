using DB;
using DB.Models;
using DB.Models.ValueObjects;
using Domain.Common.ValueObjects;
using Domain.Theme;
using Microsoft.EntityFrameworkCore;

namespace IntervalLearningApi.Services;

public class ThemeService
{
    private readonly ApplicationContext db;

    public ThemeService(ApplicationContext db)
    {
        this.db = db;
    }

    public List<Theme> GetAll() => db.Themes.AsNoTracking().ToList();

    public (bool ok, string? reason) Create(string name)
    {
        var themeTitleResult = ThemeTitle.Create(name);
        
        if (themeTitleResult.IsFailed)
            return (false, themeTitleResult.Errors.Single().Message);

        var themeTitle = themeTitleResult.Value;
        var containsSameTheme = db.Themes.SingleOrDefault(t => EF.Functions.ILike(t.Name, themeTitle.Value));

        if (containsSameTheme != null)
            return (false, "Conflict");

        var theme = new Theme(ThemeId.CreateEmpty())
        {
            Name = themeTitle
        };
        db.Themes.Add(theme);
        db.SaveChanges();

        return (true, null);
    }
}