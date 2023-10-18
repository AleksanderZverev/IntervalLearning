using DB;
using DB.DependencyInjection;
using DB.Models;
using DB.Models.Dictionary;
using IntervalLearningApi.IntegrationTests.Common.Constants;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Testcontainers.PostgreSql;

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
        var languageEntry = db.Languages.Add(new LanguageEntity()
        {
            Name = "Test English",
            NativeLanguageName = "Test English",
        });
        await db.SaveChangesAsync();
        TestConstants.Language.TestId = languageEntry.Entity.Id;

        var themeEntry = db.Themes.Add(new ThemeEntity("Test English")
        {
            LanguageId = languageEntry.Entity.Id,
        });
        await db.SaveChangesAsync();
        TestConstants.Theme.TestId = themeEntry.Entity.Id;
    } 
}