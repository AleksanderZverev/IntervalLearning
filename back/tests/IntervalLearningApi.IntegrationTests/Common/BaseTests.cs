using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Reflection;
using DB;
using DB.DependencyInjection;
using DB.Models;
using DB.Models.Dictionary;
using IntervalLearningApi.Constants;
using IntervalLearningApi.IntegrationTests.Common.Attributes;
using IntervalLearningApi.IntegrationTests.Common.Constants;
using IntervalLearningApi.IntegrationTests.Common.Extensions;
using IntervalLearningApi.Models;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Testcontainers.PostgreSql;

namespace IntervalLearningApi.IntegrationTests.Common;

[TestFixture]
public class BaseTests
{
    private WebApplicationFactory<Program> appFactory;
    protected HttpClient client;
    protected string hostPath; 

    private PostgreSqlContainer _container;

    protected IServiceScope GetScope() 
        => appFactory.Server.Services.CreateScope();

    [OneTimeSetUp]
    public async Task OneTimeSetUp()
    {
        _container = new PostgreSqlBuilder().Build();
        await _container.StartAsync();
        
        appFactory = new WebApplicationFactory<Program>().WithWebHostBuilder(b =>
        {
            b.ConfigureServices(s =>
            {
                s.RemoveAll(typeof(DbContextOptions<ApplicationContext>));
                s.RemoveAll(typeof(DbContextOptions));
                s.AddPersistence(dbBuilder =>
                {
                    dbBuilder.UseNpgsql(_container.GetConnectionString());
                });
            });
        });
        client = appFactory.CreateClient();
        hostPath = client.BaseAddress.AbsoluteUri;

        using var scope = GetScope();
        
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationContext>();
        await SetupDatabaseAsync(dbContext);

        var type = GetType();
        
        var basePathAttribute = type.GetCustomAttribute<UseBasePath>();

        if (basePathAttribute != null && !string.IsNullOrEmpty(basePathAttribute.BasePath))
        {
            client.BaseAddress = new Uri(hostPath + basePathAttribute.BasePath + "/");    
        }
        
        var testUserAttribute = type.GetCustomAttribute<UseDefaultTestUser>();

        if (testUserAttribute != null)
        { 
            await SetupTestUserAsync(
                testUserAttribute.Email,
                testUserAttribute.Password,
                testUserAttribute.FirstName,
                testUserAttribute.LastName);
        }
    }

    protected virtual async Task SetupTestUserAsync(string email, string password, string firstName, string lastName)
    {
        var register = await client.PostAsJsonAsync(
            hostPath + ApiRoutes.Accounts.BasePath + "/" + ApiRoutes.Accounts.Register, new RegisterRequest()
            {
                Email = email,
                Password = password,
                FirstName = firstName,
                LastName = lastName,
                SuggestLanguageId = TestConstants.Language.TestId,
            });

        var authResponse = await client.PostAsJsonAsync(hostPath + ApiRoutes.Accounts.BasePath  + "/" + ApiRoutes.Accounts.Authenticate, new AuthenticateRequest()
        {
            Email = email,
            Password = password,
        });

        var auth = authResponse.ToResponseDto<AuthenticateResponse>();
        if (auth == null || string.IsNullOrEmpty(auth.JwtToken))
            throw new InvalidOperationException("Unable to authenticate test user");
        
        TestConstants.User.Id = long.Parse(auth.Id);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth.JwtToken);
    }

    protected virtual async Task SetupDatabaseAsync(ApplicationContext db)
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

    [OneTimeTearDown]
    public async Task OneTimeTearDown()
    {
        await _container.StopAsync();
    }

    protected async Task AuthenticateAsync()
    {
        // var email = "test@mail.ru";
        // var password = "test123";
        //
        // var response = await client.PostAsJsonAsync(ApiRoutes.Accounts.Authenticate, new AuthenticateRequest()
        // {
        //     Email = email,
        //     Password = password,
        // });
    }
}