using DB;
using DB.DependencyInjection;
using Domain.Dictionary.Translation;
using Domain.Dictionary.Translation.ValueObjects;
using Domain.Dictionary.Word;
using Domain.Dictionary.Word.ValueObjects;
using Domain.Language;
using Domain.Language.ValueObjects;
using Domain.Theme.ValueObjects;
using IntervalLearningApi.IntegrationTests.Common.Constants;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Testcontainers.PostgreSql;
using Theme = Domain.Theme.Theme;

namespace IntervalLearningApi.IntegrationTests.Common.Api;

public class DockerIntervalLearningApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private PostgreSqlContainer _container = new PostgreSqlBuilder().Build();

    protected override void ConfigureWebHost(IWebHostBuilder b)
    {
        base.ConfigureWebHost(b);

        b.ConfigureServices(s =>
        {
            s.RemoveAll(typeof(DbContextOptions<ApplicationContext>));
            s.RemoveAll(typeof(DbContextOptions));
            s.AddDbContext<ApplicationContext>(dbBuilder =>
            {
                NpgsqlDbContextOptionsBuilderExtensions.UseNpgsql(dbBuilder, _container.GetConnectionString());
            });
        });
    }

    public async Task InitializeAsync()
    {
        await _container.StartAsync();
        
        using var scope = GetScope();
        
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationContext>();
        await SetupDatabaseAsync(dbContext);
    }

    public async Task DisposeAsync()
    {
        await _container.StopAsync();
    }
    
    public IServiceScope GetScope() 
        => Server.Services.CreateScope();
    
    private async Task SetupDatabaseAsync(ApplicationContext db)
    {
        //Base
        var english = db.Languages.Single(l => EF.Functions.ILike(l.Name, "english"));
        var russian =  db.Languages.Single(l => EF.Functions.ILike(l.Name, "russian"));
        TestConstants.Language.TestId = english.Id;
        TestConstants.Language.SuggestTranslationLanguageId = russian.Id;

        var themeEntry = db.Themes.Add(new Theme(ThemeId.Create(1).Value)
        {
            Name = ThemeTitle.Create("Test English").Value,
            LanguageId = english.Id,
        });
        await db.SaveChangesAsync();
        TestConstants.Theme.TestId = themeEntry.Entity.Id;
        
        //Dictionary
        db.Words.AddRange(new LanguageWord()
        {
            Id = 1,
            LanguageId = english.Id,
            Word = WordText.Create("hello").Value,
            Pronunciation = WordPronunciation.Create("həˈləʊ").Value,
        },
        new LanguageWord()
        {
            Id = 2,
            LanguageId = english.Id,
            Word = WordText.Create("world").Value,
            Pronunciation = WordPronunciation.Create("wɜːld").Value,
        });
        await db.SaveChangesAsync();
        
        db.Translations.AddRange(new WordTranslation()
        {
            Id = 1,
            WordId = 1,
            LanguageId = russian.Id,
            Translation = TranslationText.Create("привет").Value,
        },
        new WordTranslation()
        {
            Id = 2,
            WordId = 2,
            LanguageId = russian.Id,
            Translation = TranslationText.Create("мир").Value,
        });
        await db.SaveChangesAsync();
    } 
}