using System.Net.Http.Headers;
using System.Reflection;
using Bogus;
using IntervalLearningApi.IntegrationTests.Common.Constants;
using IntervalLearningApi.IntegrationTests.Common.Fakers;
using IntervalLearningApi.Models.ByUser;
using Microsoft.AspNetCore.Mvc.Testing;

namespace IntervalLearningApi.IntegrationTests.Common.TestScopes;

public class BaseApiTests : IAsyncLifetime
{
    private readonly WebApplicationFactory<Program> apiFactory;
    private string HostPath = "";
    private Uri? BaseAddress = null;
    
    public BaseApiTests(WebApplicationFactory<Program> apiFactory)
    {
        this.apiFactory = apiFactory;
    }

    public virtual async Task InitializeAsync()
    {
        // await apiFactory.InitializeAsync();
        var client = apiFactory.CreateClient();
        HostPath = client.BaseAddress.AbsoluteUri;
        
        var type = GetType();
        
        var basePathAttribute = type.GetCustomAttribute<UseBasePath>();
        
        if (basePathAttribute != null && !string.IsNullOrEmpty(basePathAttribute.BasePath))
        {
            BaseAddress = new Uri(HostPath + basePathAttribute.BasePath + "/");
        }
    }

    public virtual async Task DisposeAsync()
    {
        // await apiFactory.DisposeAsync();
    }

    public record TestUserInfo(
        string Id,
        string Email,
        string Password,
        string FirstName,
        string LastName);

    public record Scope(HttpClient Client, TestUserInfo User);

    public record EmptyScope(HttpClient Client);
    protected UserInfo SharedUserInfo;

    private string JoinQueryPath(string[] paths)
        => string.Join("/", paths.Select(path => path.TrimStart('/')));

    protected string AbsoluteQuery(params string[] paths)
        => HostPath + JoinQueryPath(paths);

    private void SetUpBasePath(HttpClient client, string apiBasePath)
    {
        if (!string.IsNullOrEmpty(apiBasePath))
        {
            client.BaseAddress = new Uri(HostPath + apiBasePath + "/");
        }
        else if (BaseAddress != null)
        {
            client.BaseAddress = BaseAddress;
        }
    }

    public async Task<HttpClient> GetEmptyClient(string apiBasePath = "")
    {
        var client = apiFactory.CreateClient();
        SetUpBasePath(client, apiBasePath);
        return client;
    }

    public async Task<Scope> GetRandomUserScope(string apiBasePath = "")
    {
        var client = apiFactory.CreateClient();
        SetUpBasePath(client, apiBasePath);

        var faker = new Faker();
        var testUserInfo = new TestUserInfo(
            "0",
            faker.Person.Email,
            faker.Internet.Password(),
            faker.Person.FirstName,
            faker.Person.LastName);

        var response = await client.PostAsJsonAsync(
            AbsoluteQuery(ApiRoutes.Accounts.BasePath, ApiRoutes.Accounts.Register),
            new RegisterRequest()
            {
                Email = testUserInfo.Email,
                Password = testUserInfo.Password,
                FirstName = testUserInfo.FirstName,
                LastName = testUserInfo.LastName,
                SuggestLanguageId = TestConstants.Language.TestId,
            });

        var authResponse = await AuthorizeClientAsync(client, testUserInfo.Email, testUserInfo.Password);
        return new Scope(client, testUserInfo with { Id = authResponse.Id});
    }
    
    protected async Task<AuthenticateResponse> AuthorizeClientAsync(HttpClient client, string email, string password)
    {
        var authResponse = await client.PostAsJsonAsync(
            AbsoluteQuery(ApiRoutes.Accounts.BasePath, ApiRoutes.Accounts.Authenticate),
            new AuthenticateRequest()
            {
                Email = email,
                Password = password,
            });

        var auth = authResponse.ToResponseDto<AuthenticateResponse>();
        
        if (auth == null || string.IsNullOrEmpty(auth.JwtToken))
            throw new InvalidOperationException("Unable to authenticate test user");
        
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth.JwtToken);
        return auth;
    }

    protected async Task<HttpResponseMessage> RegisterUserAsync(HttpClient client, string email, string password)
    {
        var user = new UserInfoFaker().Generate();
        var response = await client.PostAsJsonAsync(
            AbsoluteQuery(ApiRoutes.Accounts.BasePath, ApiRoutes.Accounts.Register),
            new RegisterRequest()
            {
                Email = email,
                Password = password,
                FirstName = user.FirstName,
                LastName = user.LastName,
                SuggestLanguageId = TestConstants.Language.TestId,
            });
        
        return response;
    }

    protected async Task<(string Email, string Password, HttpResponseMessage Response)> RegisterRandomUserAsync(HttpClient client)
    {
        var faker = new Faker();
        var email = faker.Person.Email;
        var password = faker.Internet.Password();
        var response = await RegisterUserAsync(client, email, password);
        return (email, password, response);
    }
}