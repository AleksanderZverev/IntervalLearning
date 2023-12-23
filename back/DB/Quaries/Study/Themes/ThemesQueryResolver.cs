using Application.Common.Interfaces.DB.Queries.Study.Themes;
using Domain.Theme;
using Domain.Theme.ValueObjects;
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