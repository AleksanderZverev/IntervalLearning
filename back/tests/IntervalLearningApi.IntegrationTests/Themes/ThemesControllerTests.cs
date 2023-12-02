using IntervalLearningApi.Controllers.Study.Themes.DTOs;

namespace IntervalLearningApi.IntegrationTests.Themes;

[UseBasePath(ApiRoutes.Themes.BasePath)]
public class ThemesControllerTests : SharedApiTests
{
    public ThemesControllerTests(SharedDockerIntervalLearningApiFactory apiFactory) : base(apiFactory)
    {
    }
    
    [Fact]
    public async Task GetAll_ShouldReturnAllThemes()
    {
        //Arrange
        var (client, scope) = SharedScope;

        //Act
        var allThemesResult = await client.GetAsync(ApiRoutes.Themes.Get_GetAll);
        var allThemes = allThemesResult.ToResponseDto<List<ThemeDto>>();

        //Assert
        allThemes.Should().NotBeNull().And.NotBeEmpty();
        var theme = allThemes.First();
        theme.Id.Should().NotBe(0);
        theme.Name.Should().NotBeEmpty();
    }
}