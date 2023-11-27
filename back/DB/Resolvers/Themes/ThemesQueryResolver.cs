using Application.Common.Interfaces.Domain.Themes;
using DB.Models.ValueObjects;
using Domain.Theme;

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
}