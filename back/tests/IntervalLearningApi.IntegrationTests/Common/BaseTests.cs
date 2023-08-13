using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Reflection;
using IntervalLearningApi.Constants;
using IntervalLearningApi.IntegrationTests.Common.Attributes;
using IntervalLearningApi.IntegrationTests.Common.Extensions;
using IntervalLearningApi.Models;
using Microsoft.AspNetCore.Mvc.Testing;

namespace IntervalLearningApi.IntegrationTests.Common;

[TestFixture]
public class BaseTests
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
    
    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        var type = GetType();

        var hostPath = client.BaseAddress.AbsoluteUri;
        
        var basePathAttribute = type.GetCustomAttribute<UseBasePath>();

        if (basePathAttribute != null && !string.IsNullOrEmpty(basePathAttribute.BasePath))
        {
            client.BaseAddress = new Uri(hostPath + basePathAttribute.BasePath + "/");    
        }
        
        var testUserAttribute = type.GetCustomAttribute<UseDefaultTestUser>();

        if (testUserAttribute != null)
        {
            var authResponse =  client.PostAsJsonAsync(hostPath + ApiRoutes.Accounts.BasePath  + "/" + ApiRoutes.Accounts.Authenticate, new AuthenticateRequest()
            {
                Email = testUserAttribute.Email,
                Password = testUserAttribute.Password,
            }).GetAwaiter().GetResult();

            var auth = authResponse.ToResponseDto<AuthenticateResponse>();
            if (auth == null || string.IsNullOrEmpty(auth.JwtToken))
                throw new InvalidOperationException("Unable to authenticate test user");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth.JwtToken);
        }
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