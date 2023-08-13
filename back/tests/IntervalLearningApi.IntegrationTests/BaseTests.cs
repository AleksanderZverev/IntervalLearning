using System.Net.Http.Json;
using IntervalLearningApi.Constants;
using IntervalLearningApi.Models;
using IntervalLearningApi.Models.Common;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Xunit;

namespace IntervalLearningApi.IntegrationTests;

public class FakeDateTimeProvider : IDateTimeProvider
{
    public DateTime UtcNow => currentTime;
    
    private DateTime currentTime = DateTime.UtcNow;

    public void SetTime(DateTime dateTime)
    {
        currentTime = dateTime;
    }
}

[TestFixture]
public class BaseTests //: IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> appFactory;
    protected readonly HttpClient client;
    
    public BaseTests()
    {
        appFactory = new WebApplicationFactory<Program>().WithWebHostBuilder(b =>
        {
            b.ConfigureServices(s =>
            {
                
            });
        });
        client = appFactory.CreateClient();
    }

    // protected record ScopeData(HttpClient Client, FakeDateTimeProvider DateTime);
    //
    // protected ScopeData Scope()
    // {
    //     var dateTimeProvider = new FakeDateTimeProvider();
    //     var scopeFactory = new WebApplicationFactory<Program>().WithWebHostBuilder(b =>
    //     {
    //         b.ConfigureServices(s =>
    //         {
    //             s.RemoveAll(typeof(IDateTimeProvider));
    //             s.AddSingleton<IDateTimeProvider>(dateTimeProvider);
    //         });
    //     });
    //     
    //     var scopeClient = scopeFactory.CreateClient();
    //     return new ScopeData(scopeClient, dateTimeProvider);
    // }

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