using Bogus;
using IntervalLearningApi.IntegrationTests.Common.Constants;
using IntervalLearningApi.Models.ByUser;

namespace IntervalLearningApi.IntegrationTests.Collections;

[UseBasePath(ApiRoutes.Collections.BasePath)]
public class CollectionsControllerTests : SharedApiTests
{
    public CollectionsControllerTests(IntervalLearningApiFactory apiFactory) : base(apiFactory)
    {
    }
    
    [Fact]
    public async Task GetAll_ShouldReturnEmptyCollection_WhenNewUserRegistered()
    {
        var (client, scope) = SharedScope;

        var getAllResponse = await client.GetAsync(ApiRoutes.Collections.GetAll);
        
        var allCollections = getAllResponse.ToResponseDto<List<Collection>>();
        allCollections.Should().NotBeNull().And.BeEmpty();
    }

    [Fact]
    public async Task GetAll_ShouldReturnAllCollections()
    {
        //Arrange
        var (client, scope) = SharedScope;
        var addedCollections = await CreateRandomCollectionsAsync(10);

        //Act
        var getAllResponse = await client.GetAsync(ApiRoutes.Collections.GetAll);
        var allCollections = getAllResponse.ToResponseDto<List<Collection>>();

        //Assert
        allCollections.Should().NotBeNull().And.NotBeEmpty();
        allCollections.Count.Should().Be(addedCollections.Count);
        allCollections.Select(c => c.Title).Should().BeEquivalentTo(addedCollections.Select(c => c.Title));
    }

    //TODO: Update logic
    [Theory]
    [InlineData("New collection")]
    [InlineData("My_Simple_Collection")]
    public async Task CreateCollection_ShouldCreate(string collectionName)
    {
        //Arrange
        var (client, scope) = SharedScope;

        //Act
        var createResponse = await client.PostAsJsonAsync(
            ApiRoutes.Collections.Create,
            new CreateCollectionItem()
            {
                Title = collectionName,
                IsDefaultBackSide = false,
                ThemeId = TestConstants.Theme.TestId,
            });

        //Assert
        createResponse.IsSuccessStatusCode.Should().BeTrue();
        var addedCollection = createResponse.ToResponseDto<Collection>();

        addedCollection.Should().NotBeNull();
        addedCollection.Title.Should().BeEquivalentTo(collectionName);
        
        // AddedCollections.Add(addedCollection);
    }

    public static IEnumerable<object[]> IncorrectNames = new object[][]
    {
        new[] { "" },
        new[] { new string('a', 300) },
    };
    
    [Theory]
    [MemberData(nameof(IncorrectNames))]
    public async Task CreateCollection_ShouldFailOnIncorrectName(string collectionName)
    {
        //Arrange
        var (client, scope) = SharedScope;

        //Act
        var createResponse = await client.PostAsJsonAsync(
            ApiRoutes.Collections.Create,
            new CreateCollectionItem()
            {
                Title = collectionName,
                IsDefaultBackSide = false,
                ThemeId = TestConstants.Theme.TestId,
            });

        //Assert
        createResponse.IsSuccessStatusCode.Should().BeFalse();
    }

    [Fact]
    public async Task GetCollection_ShouldReturnExistingCollections()
    {
        //Arrange
        var (client, scope) = SharedScope;
        var addedCollections = await CreateRandomCollectionsAsync(10);

        //Act
        foreach (var testCollection in addedCollections)
        {
            var getCollectionResponse = await client.GetAsync(
                ApiRoutes.Collections.GetCollectionPath(
                        short.Parse(testCollection.Id)));
            var collection = getCollectionResponse.ToResponseDto<Collection>();
            
            //Assert
            collection.Id.Should().Be(testCollection.Id.ToString());
            collection.Title.Should().Be(testCollection.Title);
            collection.ThemeId.Should().Be(testCollection.ThemeId);
            collection.IsPublic.Should().Be(testCollection.IsPublic);
            collection.CardsCount.Should().Be(testCollection.CardsCount);
        }
    }

    [Fact]
    public async Task GetCollection_ShouldReturnFailOnUnknownCollection()
    {
        //Arrange
        var (client, scope) = SharedScope;
        var fakeCollectionId = new Faker().Random.Short();

        //Act
        var getCollectionResponse = await client.GetAsync(
            ApiRoutes.Collections.GetCollectionPath(
                fakeCollectionId));

        //Assert
        getCollectionResponse.IsSuccessStatusCode.Should().BeFalse();
    }

    [Theory]
    [InlineData("Check_and_mate", "chec")]
    [InlineData("Mathematics", "mat")]
    [InlineData("My collections", "My c")]
    public async Task SearchCollection_ShouldSearchByName(string collectionFullName, string searchRequestName)
    {
        //Arrange
        var (client, scope) = SharedScope;
        var addedCollection = await CreateCollectionAsync(collectionFullName);

        //Act
        var searchResponse = await client.GetAsync(
            ApiRoutes.Collections.SearchPrivate +
            new QueryString()
                //TODO: move Theme Id to class property 
                .Add("themeId", TestConstants.Theme.TestId.ToString())
                .Add("searchName", searchRequestName));
        var searchResult = searchResponse.ToResponseDto<List<Collection>>();

        //Assert
        searchResult.Should().NotBeNull().And.NotBeEmpty();
        searchResult.Select(c => c.Title).Should().ContainSingle(collectionFullName);
    }

    [Fact]
    public async Task SearchCollection_ShouldSearchByPages()
    {
        //Arrange
        var (client, scope) = SharedScope;
        var countPerPage = 4;
        var pages = 6;
        var preAddedCollections = await CreateRandomCollectionsAsync(countPerPage * pages);

        //Act
        var pageToCollections = new List<(int Page, List<Collection>? Collections)>();
        for (var i = 0; i < pages; i++)
        {
            var pageNumber = i + 1;
            
            var pageResponse = await client.GetAsync(
                ApiRoutes.Collections.SearchPrivate +
                new QueryString()
                    .Add("themeId", TestConstants.Theme.TestId.ToString())
                        .Add("page", pageNumber.ToString())
                    .Add("count", countPerPage.ToString()));
            var pageCollections = pageResponse.ToResponseDto<List<Collection>>();
            pageToCollections.Add((pageNumber, pageCollections));
        }
        
        //Assert
        pageToCollections.Should().NotBeEmpty();
        pageToCollections.Count.Should().Be(pages);
        pageToCollections.Select(t => t.Collections).Should().AllSatisfy(c => c.Count.Should().Be(countPerPage));
        

        var allCollections = pageToCollections.SelectMany(t => t.Collections).ToList();
        
        allCollections.Select(c => c.Id).Should().OnlyHaveUniqueItems();
        allCollections.Select(c => c.Title).Should().OnlyHaveUniqueItems();
        allCollections.Select(c => c.Title).Should().BeSubsetOf(preAddedCollections.Select(c => c.Title));
    }

    [Fact]
    public async Task SearchCollection_ShouldReturnSameValueForThePage()
    {
        //Arrange
        var (client, scope) = SharedScope;
        var countPerPage = 4;
        var pages = 5;
        var preAddedCollections = await CreateRandomCollectionsAsync(countPerPage * pages);
        
        //Act
        var pageNumber = Random.Shared.Next(1, pages + 1);
        var fistPageResponse = await client.GetAsync(
            ApiRoutes.Collections.SearchPrivate +
            new QueryString()
                .Add("themeId", TestConstants.Theme.TestId.ToString())
                .Add("page", pageNumber.ToString())
                .Add("count", countPerPage.ToString()));
        var firstPageCollections = fistPageResponse.ToResponseDto<List<Collection>>();

        var secondPageResponse = await client.GetAsync(
            ApiRoutes.Collections.SearchPrivate +
            new QueryString()
                .Add("themeId", TestConstants.Theme.TestId.ToString())
                .Add("page", pageNumber.ToString())
                .Add("count", countPerPage.ToString()));
        var secondPageCollections = secondPageResponse.ToResponseDto<List<Collection>>();

        //Assert
        firstPageCollections.Should().NotBeNull().And.NotBeEmpty();
        secondPageCollections.Should().NotBeNull().And.NotBeEmpty();
        firstPageCollections.Select(c => c.Id).Should().Equal(secondPageCollections.Select(c => c.Id));
        firstPageCollections.Select(c => c.Title).Should().Equal(secondPageCollections.Select(c => c.Title));
        
        firstPageCollections.Select(c => c.Title).Should().BeSubsetOf(preAddedCollections.Select(c => c.Title));
    }
    
    [Theory]
    [InlineData("My super collection", "Unknown")]
    [InlineData("My_super_collection", "Unknown")]
    public async Task SearchCollection_ShouldReturnEmpty_IfNothingFound(string collectionFullName, string searchRequestName)
    {
        //Arrange
        var (client, scope) = SharedScope;
        await CreateCollectionAsync(collectionFullName);

        //Act
        var searchResponse = await client.GetAsync(
            ApiRoutes.Collections.SearchPrivate +
            new QueryString()
                //TODO: move Theme Id to class property 
                .Add("themeId", TestConstants.Theme.TestId.ToString())
                .Add("searchName", searchRequestName));
        var searchResult = searchResponse.ToResponseDto<List<Collection>>();

        //Assert
        searchResult.Should().NotBeNull().And.BeEmpty();
    }

    [Theory]
    [InlineData("")]
    [InlineData("Aaa")]
    [InlineData("My super collection")]
    [InlineData("My_super_collection")]
    public async Task SearchCollection_ShouldReturnEmpty_IfNoCollections(string searchRequestName)
    {
        //Arrange
        var (client, scope) = SharedScope;

        //Act
        var searchResponse = await client.GetAsync(
            ApiRoutes.Collections.SearchPrivate +
            new QueryString()
                //TODO: move Theme Id to class property 
                .Add("themeId", TestConstants.Theme.TestId.ToString())
                .Add("searchName", searchRequestName));
        var searchResult = searchResponse.ToResponseDto<List<Collection>>();

        //Assert
        searchResult.Should().NotBeNull().And.BeEmpty();
    }
    
    public static IEnumerable<object[]> IncorrectSearchRequests = new object[][]
    {
        new[] { new string('a', 300) },
    };
    
    [Theory]
    [MemberData(nameof(IncorrectSearchRequests))]
    public async Task SearchCollection_ShouldReturnFailOnIncorrectInput(string searchRequestName)
    {
        //Arrange
        var (client, scope) = SharedScope;
        await CreateRandomCollectionsAsync(10);

        //Act
        var searchResponse = await client.GetAsync(
            ApiRoutes.Collections.SearchPrivate +
            new QueryString()
                //TODO: move Theme Id to class property 
                .Add("themeId", TestConstants.Theme.TestId.ToString())
                .Add("searchName", searchRequestName));
        var searchResult = searchResponse.ToResponseDto<List<Collection>>();

        //Assert
        searchResult.Should().NotBeNull().And.BeEmpty();
    }
}