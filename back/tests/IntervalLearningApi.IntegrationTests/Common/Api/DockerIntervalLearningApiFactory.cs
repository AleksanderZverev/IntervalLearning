using DB;
using DB.DependencyInjection;
using DB.Models;
using DB.Models.Dictionary;
using DB.Models.ValueObjects;
using Domain.Language;
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
            s.AddPersistence(dbBuilder =>
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
        var languageEntry = db.Languages.Add(
            Language.CreateNew("Test English", "Test English").Value);
        await db.SaveChangesAsync();
        TestConstants.Language.TestId = languageEntry.Entity.Id;

        var themeEntry = db.Themes.Add(new Theme(ThemeId.Create(1).Value)
        {
            Name = ThemeTitle.Create("Test English").Value,
            LanguageId = languageEntry.Entity.Id,
        });
        await db.SaveChangesAsync();
        TestConstants.Theme.TestId = themeEntry.Entity.Id;
    } 
}