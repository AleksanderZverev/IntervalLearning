namespace IntervalLearningApi.IntegrationTests.User;

[UseBasePath(ApiRoutes.Accounts.BasePath)]
public class AuthenticationControllerTests : ScopeApiTests
{
    public AuthenticationControllerTests(IntervalLearningApiFactory apiFactory) 
        : base(apiFactory)
    {
    }
    
    [Fact]
    public async Task Register_ShouldRegisterUser()
    {
        //Arrange
        var client = await GetEmptyClient();

        //Act
        var registration = await RegisterRandomUserAsync(client);

        //Assert
        registration.Response.IsSuccessStatusCode.Should().BeTrue();
    }

    [Fact]
    public async Task Authenticate_ShouldAuthenticateRegisteredPerson()
    {
        //Arrange
        var client = await GetEmptyClient();
        var (email, password, _) = await RegisterRandomUserAsync(client);
        
        //Act
        var response = await client.PostAsJsonAsync(ApiRoutes.Accounts.Authenticate, new AuthenticateRequest()
        {
            Email = email,
            Password = password,
        });

        //Assert
        response.IsSuccessStatusCode.Should().BeTrue();
        var responseModel = response.ToResponseDto<AuthenticateResponse>();
        
        responseModel.Should().NotBeNull();
        responseModel.Email.Should().BeEquivalentTo(email);
        responseModel.JwtToken.Should().NotBeEmpty();
        responseModel.RefreshToken.Should().NotBeEmpty();
    }
    
    [Fact]
    public async Task RefreshToken_ShouldNotRefreshTokenNotExpiredToken()
    {
        //Arrange
        var client = await GetEmptyClient();
        var (email, password, _) = await RegisterRandomUserAsync(client);
        var authResponse = await client.PostAsJsonAsync(ApiRoutes.Accounts.Authenticate, new AuthenticateRequest()
        {
            Email = email,
            Password = password,
        });
        var oldAuth = authResponse.ToResponseDto<AuthenticateResponse>();
        
        //Act
        var refreshResponse = await client.PostAsJsonAsync(ApiRoutes.Accounts.RefreshToken, new AuthenticateRequest()
        {
            Email = email,
            Password = password,
        });
        var newAuth = authResponse.ToResponseDto<AuthenticateResponse>();

        //Assert
        oldAuth.Should().NotBeNull();
        oldAuth.JwtToken.Should().NotBeEmpty();
        
        newAuth.Should().NotBeNull();
        newAuth.JwtToken.Should().NotBeNull();
        oldAuth.JwtToken.Should().BeEquivalentTo(newAuth.JwtToken);
    }
}