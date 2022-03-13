using DB;
using DB.Models;
using Microsoft.EntityFrameworkCore;

namespace IntervalLearningApi.Services;

public class ThemeService
{
    private readonly ApplicationContext db;

    public ThemeService(ApplicationContext db)
    {
        this.db = db;
    }

    public List<ThemeEntity> GetAll() => db.Themes.AsNoTracking().ToList();

    public (bool ok, string? reason) Create(string name)
    {
        var lowerName = name.ToLowerInvariant();

        var containsSameTheme = db.Themes.SingleOrDefault(t => t.Name == lowerName);

        if (containsSameTheme != null)
            return (false, "Conflict");

        var theme = new ThemeEntity(lowerName);
        db.Themes.Add(theme);
        db.SaveChanges();

        return (true, null);
    }
}