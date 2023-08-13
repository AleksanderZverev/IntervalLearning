using System.Net.Http.Json;
using FluentAssertions;
using IntervalLearningApi.Constants;
using IntervalLearningApi.IntegrationTests.Common;
using IntervalLearningApi.IntegrationTests.Common.Attributes;
using IntervalLearningApi.IntegrationTests.Common.Extensions;
using IntervalLearningApi.Models;

namespace IntervalLearningApi.IntegrationTests.User;

[UseBasePath(ApiRoutes.Accounts.BasePath)]
public class AuthenticationControllerTests : BaseTests
{
    [Test]
    public async Task Register_ShouldRegisterUser()
    {
        var response = await client.PostAsJsonAsync(ApiRoutes.Accounts.Register, new RegisterRequest()
        {
            Email = "test@mail.ru",
            Password = "test",
            FirstName = "Alex",
            SuggestLanguageId = 1,
        });

        response.IsSuccessStatusCode.Should().BeTrue();
    }

    [Test]
    public async Task Authenticate_ShouldAuthenticateExistingPerson()
    {
        var email = "test@mail.ru";
        var password = "test123";
        var response = await client.PostAsJsonAsync(ApiRoutes.Accounts.Authenticate, new AuthenticateRequest()
        {
            Email = email,
            Password = password,
        });

        response.IsSuccessStatusCode.Should().BeTrue();

        var responseModel = response.ToResponseDto<AuthenticateResponse>();
        
        responseModel.Should().NotBeNull();
        responseModel.Email.Should().Be(email);
        responseModel.JwtToken.Should().NotBeEmpty();
        responseModel.RefreshToken.Should().NotBeEmpty();
    }
    
    [Test]
    public async Task RefreshToken_ShouldNotRefreshTokenNotExpiredToken()
    {
        var email = "test@mail.ru";
        var password = "test123";
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