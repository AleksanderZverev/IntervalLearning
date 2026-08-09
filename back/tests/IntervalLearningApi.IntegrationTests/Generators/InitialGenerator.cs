using DB;
using Domain.Common.ValueObjects;
using Domain.Language;
using Domain.Language.ValueObjects;
using Domain.Schedule;
using Domain.Schedule.Entities.Phase;
using Domain.Schedule.Entities.Phase.ValueObjects;
using Domain.Schedule.ValueObjects;
using Domain.Theme;
using Domain.Theme.ValueObjects;
using Domain.User.ValueObjects;
using Microsoft.Extensions.DependencyInjection;

namespace IntervalLearningApi.IntegrationTests.Generators;

public class InitialGenerator : LocalApiTests
{
    public InitialGenerator(LocalIntervalLearningApiFactory apiFactory) : base(apiFactory)
    {
    }

    [Theory(Skip = "FOR CUSTOM USE ONLY")]
    [InlineData("test@mail.ru", "test123")]
    public async Task GenerateInitialData(string email, string password)
    {
        var sp = GetServiceProvider();
        var db = sp.GetRequiredService<ApplicationContext>();

        var client = await GetEmptyClient();
        
        var userEmail = EmailAddress.Create(email).Value;
        if (!db.Users.Any(u => u.Email == userEmail))
        {
            await RegisterUserAsync(client, email, password);
        }
        
        var userInfo = await AuthorizeClientAsync(client, email, password);

        var userId = UserId.Create(long.Parse(userInfo.Id)).Value;

        AddLanguage(db, 1, "English", "English", "https://wooordhunt.ru/word/[word]", "wooordhunt");
        AddLanguage(db, 2, "Russian", "Русский", null, null);
        AddLanguage(db, 3, "Japanese", "日本語", "https://jisho.org/search/[word]", "jisho");

        AddTheme(db, 1, "english", 1);
        AddTheme(db, 2, "japanese", 3);

        AddLearningSchedule(db, userId, 1, "Default", 1, 3, 7, 14, 28, 56, 56, 56);
    }

    private void AddLearningSchedule(
        ApplicationContext db,
        UserId userId,
        int id,
        string name,
        params int[] daysIntervals)
    {
        var scheduleId = ScheduleId.Create((short)id).Value;

        if (db.RepeatsSchedules.Any(s => s.Id == scheduleId))
            return;

        db.RepeatsSchedules.Add(
            new RepeatsSchedule(userId, scheduleId)
            {
                CardsCountPerPhase = 30,
                ForgottenBehavior = ForgottenBehavior.MoveToPreviousStep,
                Title = ScheduleTitle.Create(name).Value,
                Phases = daysIntervals
                    .Select((daysInterval, index) =>
                        new Phase(scheduleId, userId, PhaseId.Create((short)(index + 1)).Value)
                        {
                            SecondsFromLastPhase = (uint)TimeSpan.FromDays(daysInterval).TotalSeconds,
                        })
                    .ToList(),
            });
        db.SaveChanges();
    }

    private void AddLanguage(
        ApplicationContext db,
        int id,
        string name,
        string nativeName,
        string? link,
        string? linkTitle)
    {
        var languageId = LanguageId.Create((short)id).Value;
        var languageName = ShortString.Create(name).Value;

        if (db.Languages.Any(l => l.Id == languageId || l.Name == languageName))
            return;

        db.Languages.Add(Language.Create((short)id, name, nativeName, link, linkTitle).Value);
        db.SaveChanges();
    }

    private void AddTheme(ApplicationContext db, int id, string themeName, int languageId)
    {
        var themeNameVO = ThemeTitle.Create(themeName).Value;
        if (db.Themes.Any(t => t.Id == id || t.Name == themeNameVO))
            return;

        db.Themes.Add(
            new Theme(
                ThemeId.Create((short)id).Value,
                themeNameVO,
                LanguageId.Create((short)languageId).Value));
        db.SaveChanges();
    }
}