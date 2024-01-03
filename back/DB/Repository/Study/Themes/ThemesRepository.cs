using DB.Configurations.Study;
using Domain.Theme;
using Domain.Theme.ValueObjects;
using DomainServices.DB.Repositories;
using DomainServices.DB.Repositories.Study.Themes;
using FluentResults;

namespace DB.Repository.Study.Themes;

public class ThemesRepository : BaseRepository<Theme>, IRepository<Theme, ThemeId, ThemeIdParams>
{
    public ThemesRepository(ApplicationContext db) : base(db)
    {
    }

    public Result<ThemeId> GetUniqueId(ThemeIdParams param)
    {
        var seqName = ThemeConfiguration.GetSequenceName();
        const int themesStartValue = 10;
        db.EnsureSequenceCreated(seqName, themesStartValue);
        var themeId = db.GetSequenceNextValue16(seqName, themesStartValue);
        return ThemeId.Create(themeId);
    }
}