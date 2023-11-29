using Application.Common.Interfaces.Domain.Themes;
using DB.Models.ValueObjects;
using Domain.Theme;
using Microsoft.EntityFrameworkCore;

namespace DB.Resolvers.Themes;

public class ThemesQueryResolver : IThemesQueryResolver
{
    private readonly ApplicationContext db;

    public ThemesQueryResolver(ApplicationContext db)
    {
        this.db = db;
    }

    public Task<Theme?> Find(ThemeId themeId)
    {
        return db.Themes.FindAsync(themeId).AsTask();
    }

    public Task<List<Theme>> GetAll()
    {
        return db.Themes.AsNoTracking().ToListAsync();
    }

    public Task<List<Theme>> SearchByTitle(ThemeTitle title)
    {
        var titleString = title.Value;
        return db.Themes
            .Where(t => EF.Functions.ILike(t.Name, titleString))
            .ToListAsync();
    }
}