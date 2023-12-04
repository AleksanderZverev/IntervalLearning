using IntervalLearningApi.Controllers.Dictionary.DTOs;
using IntervalLearningApi.Controllers.Dictionary.Requests.AddTranslations;

namespace IntervalLearningApi.IntegrationTests.Dictionary;

[UseBasePath(ApiRoutes.Dictionary.BasePath)]
public class DictionaryControllerTests : SharedApiTests
{
    public DictionaryControllerTests(SharedDockerIntervalLearningApiFactory apiFactory) : base(apiFactory)
    {
    }
    
    [Xunit.Theory]
    [InlineData("hello")]
    [InlineData("world")]
    public async Task SearchWords_ShouldSearchByWord(string word)
    {
        //Arrange
        var (client, scope) = SharedScope;

        //Act
        var searchResponse = await client.GetAsync(
            ApiRoutes.Dictionary.Get_SearchWords +
            new QueryString().Add("word", word));
        var foundWords = searchResponse.ToResponseDto<List<WordDto>>();

        //Assert
        foundWords.Should().NotBeNullOrEmpty();
        foundWords.Select(w => w.Word).Should().Contain(word);
    }
    
    [Xunit.Theory]
    [InlineData("həˈləʊ")]
    [InlineData("wɜːld")]
    public async Task SearchWords_ShouldSearchByPronunciation(string pronunciation)
    {
        //Arrange
        var (client, scope) = SharedScope;

        //Act
        var searchResponse = await client.GetAsync(
            ApiRoutes.Dictionary.Get_SearchWords +
            new QueryString().Add("pronunciation", pronunciation));
        var foundWords = searchResponse.ToResponseDto<List<WordDto>>();

        //Assert
        foundWords.Should().NotBeNullOrEmpty();
        foundWords.Select(w => w.Pronunciation).Should().Contain(pronunciation);
    }
    
    [Fact]
    public async Task GetLanguages_ShouldReturnAllLanguages()
    {
        //Arrange
        var (client, scope) = SharedScope;

        //Act
        var languagesResponse = await client.GetAsync(ApiRoutes.Dictionary.Get_GenLanguages);
        var languages = languagesResponse.ToResponseDto<List<LanguageDto>>();

        //Assert
        languages.Should().NotBeNullOrEmpty();
        var language = languages.First();
        language.Id.Should().NotBeNullOrEmpty().And.NotBe("0");
        language.Name.Should().NotBeNullOrEmpty();
        language.NativeLanguageName.Should().NotBeNullOrEmpty();
    }
    
    [Xunit.Theory]
    [InlineData("hello")]
    [InlineData("world")]
    public async Task GetTranslation_ShouldReturnTranslationForWords(string word)
    {
        //Arrange
        var (client, scope) = SharedScope;

        //Act
        var translations = await GetTranslation(client, word);

        //Assert
        translations.Should().NotBeNullOrEmpty();
        var translation = translations.First();
        translation.Id.Should().NotBeNullOrEmpty().And.NotBe("0");
        translation.Translation.Should().NotBeNullOrEmpty();
    }
    
    [Xunit.Theory(Skip = "Implemented limited logic only for specific format")]
    [InlineData("hello", "здравствуйте")]
    [InlineData("world", "мировой")]
    public async Task AddTranslation_ShouldNewTranslations(string word, string translationText)
    {
        //Arrange
        var (client, scope) = SharedScope;
        var oldTranslations = await GetTranslation(client, word);

        //Act
        var addTranslationResponse = await client.PostAsJsonAsync(
            ApiRoutes.Dictionary.Post_AddTranslations,
            new AddTranslationsRequest()
            {
                LanguageId = 1,
                Text = translationText,
                TranslationLanguageId = 2,
            });

        //Assert
        addTranslationResponse.IsSuccessStatusCode.Should().BeTrue();
        var newTranslations = await GetTranslation(client, word);
        newTranslations.Should().NotBeNullOrEmpty();
        newTranslations.Count.Should().BeGreaterThan(oldTranslations.Count);
        newTranslations.Select(t => t.Translation).Should().Contain(translationText);
    }

    private static async Task<List<TranslationDto>?> GetTranslation(HttpClient client, string word)
    {
        var translationsResponse = await client.GetAsync(
            ApiRoutes.Dictionary.Get_GetTranslation +
            new QueryString().Add("word", word));
        var translations = translationsResponse.ToResponseDto<List<TranslationDto>>();
        return translations;
    }
}