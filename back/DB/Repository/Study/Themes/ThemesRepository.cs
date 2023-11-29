using Application.Common.Interfaces.DB.Repositories;
using Application.Common.Interfaces.DB.Repositories.Study.Themes;
using DB.Configurations.Study;
using DB.Models.ValueObjects;
using Domain.Theme;
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
        db.EnsureSequenceCreated(seqName);
        var themeId = db.GetSequenceNextValue16(seqName);
        return ThemeId.Create(themeId);
    }
}