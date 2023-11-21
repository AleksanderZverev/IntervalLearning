using DB;
using DB.Configurations.Study;
using DB.Models;
using DB.Models.ValueObjects;
using Domain.Common.ValueObjects;
using Domain.Theme;
using FluentResults;
using Infrastructure.Errors;
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

    public Result Create(string name)
    {
        var themeTitleResult = ThemeTitle.Create(name);
        
        if (themeTitleResult.IsFailed)
            return themeTitleResult.ToResult();

        var themeTitle = themeTitleResult.Value;
        var containsSameTheme = db.Themes.SingleOrDefault(t => EF.Functions.ILike(t.Name, themeTitle.Value));

        if (containsSameTheme != null)
            return new ConflictError("Theme");

        var seqName = ThemeConfiguration.GetSequenceName();
        db.EnsureSequenceCreated(seqName);
        var themeId = db.GetSequenceNextValue16(seqName);
        
        var theme = new Theme(ThemeId.Create(themeId).Value)
        {
            Name = themeTitle
        };
        db.Themes.Add(theme);
        db.SaveChanges();

        return Result.Ok();
    }
}