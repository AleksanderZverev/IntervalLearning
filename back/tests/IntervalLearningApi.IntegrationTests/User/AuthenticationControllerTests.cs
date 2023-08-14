using System.Net.Http.Json;
using Bogus;
using FluentAssertions;
using IntervalLearningApi.Constants;
using IntervalLearningApi.IntegrationTests.Common;
using IntervalLearningApi.IntegrationTests.Common.Attributes;
using IntervalLearningApi.IntegrationTests.Common.Constants;
using IntervalLearningApi.IntegrationTests.Common.Extensions;
using IntervalLearningApi.Models;

namespace IntervalLearningApi.IntegrationTests.User;

[UseBasePath(ApiRoutes.Accounts.BasePath)]
[UseDefaultTestUser]
public class AuthenticationControllerTests : BaseTests
{
    [Test]
    public async Task Register_ShouldRegisterUser()
    {
        var user = new UserFaker().Generate();
        var password = new Faker().Internet.Password();
        
        var response = await client.PostAsJsonAsync(ApiRoutes.Accounts.Register, new RegisterRequest()
        {
            Email = user.Email,
            Password =  password,
            FirstName = user.FirstName,
            LastName = user.LastName,
            SuggestLanguageId = TestConstants.Language.TestId,
        });

        response.IsSuccessStatusCode.Should().BeTrue();
    }

    [Test]
    public async Task Authenticate_ShouldAuthenticateExistingPerson()
    {
        var response = await client.PostAsJsonAsync(ApiRoutes.Accounts.Authenticate, new AuthenticateRequest()
        {
            Email = TestConstants.User.Email,
            Password = TestConstants.User.Password,
        });

        response.IsSuccessStatusCode.Should().BeTrue();

        var responseModel = response.ToResponseDto<AuthenticateResponse>();
        
        responseModel.Should().NotBeNull();
        responseModel.Email.Should().Be(TestConstants.User.Email);
        responseModel.JwtToken.Should().NotBeEmpty();
        responseModel.RefreshToken.Should().NotBeEmpty();
    }
    
    [Test]
    public async Task RefreshToken_ShouldNotRefreshTokenNotExpiredToken()
    {
        var email = TestConstants.User.Email;
        var password = TestConstants.User.Password;
        
        var response = await client.PostAsJsonAsync(ApiRoutes.Accounts.Authenticate, new AuthenticateRequest()
        {
            Email = email,
            Password = password,
        });
        var oldAuth = response.ToResponseDto<AuthenticateResponse>();
        oldAuth.Should().NotBeNull();
        oldAuth.JwtToken.Should().NotBeEmpty();
        
        var refreshResponse = await client.PostAsJsonAsync(ApiRoutes.Accounts.RefreshToken, new AuthenticateRequest()
        {
            Email = email,
            Password = password,
        });

        var newAuth = response.ToResponseDto<AuthenticateResponse>();
        newAuth.Should().NotBeNull();
        newAuth.JwtToken.Should().NotBeNull();
        
        oldAuth.JwtToken.Should().BeEquivalentTo(newAuth.JwtToken);
    }
}