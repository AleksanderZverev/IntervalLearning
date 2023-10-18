using Bogus;
using IntervalLearningApi.IntegrationTests.Common.Constants;
using IntervalLearningApi.Models.ByUser;

namespace IntervalLearningApi.IntegrationTests.Collections;

[UseBasePath(ApiRoutes.Collections.BasePath)]
public class CollectionsControllerTests : SharedApiTests
{
    public CollectionsControllerTests(SharedDockerIntervalLearningApiFactory apiFactory) : base(apiFactory)
    {
    }

    [Fact]
    public async Task GetAll_ShouldReturnEmptyCollection_WhenNewUserRegistered()
    {
        //Arrange
        var (client, scope) = SharedScope;

        //Act
        var allCollections = await GetAllCollectionsAsync();
        
        //Assert
        allCollections.Should().NotBeNull().And.BeEmpty();
    }

    [Fact]
    public async Task GetAll_ShouldReturnAllCollections()
    {
        //Arrange
        var (client, scope) = SharedScope;
        var addedCollections = await CreateRandomCollectionsAsync(10);

        //Act
        var allCollections = await GetAllCollectionsAsync();

        //Assert
        allCollections.Should().NotBeNull().And.NotBeEmpty();
        allCollections.Count.Should().Be(addedCollections.Count);
        allCollections.Select(c => c.Title).Should().BeEquivalentTo(addedCollections.Select(c => c.Title));
    }

    //TODO: Update logic
    [Theory]
    [InlineData("New collection")]
    [InlineData("My_Simple_Collection")]
    public async Task CreateCollection_ShouldReturnCreatedData(string collectionName)
    {
        //Arrange
        var (client, scope) = SharedScope;

        //Act
        var createdCollection = await CreateCollectionAsync(collectionName);

        //Assert
        createdCollection.Should().NotBeNull();
        createdCollection.Title.Should().BeEquivalentTo(collectionName);
    }
    
    [Theory]
    [InlineData("New collection")]
    [InlineData("My_Simple_Collection")]
    public async Task CreateCollection_ShouldActuallyCreate(string collectionName)
    {
        //Arrange
        var (client, scope) = SharedScope;
        var oldCollections = await GetAllCollectionsAsync();

        //Act
        await CreateCollectionAsync(collectionName);

        //Assert
        var newCollections = await GetAllCollectionsAsync();
        oldCollections.Should().BeEmpty();
        newCollections.Should().NotBeNull().And.HaveCount(1);
        newCollections.Single().Title.Should().BeEquivalentTo(collectionName);
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
        var createdCollection = await CreateCollectionAsync(collectionName);

        //Assert
        createdCollection.Should().BeNull();
    }
    
    [Theory]
    [InlineData("New collection")]
    [InlineData("My_Simple_Collection")]
    public async Task CreateCollection_ShouldReturnUpdatedData(string collectionName)
    {
        //Arrange
        var (client, scope) = SharedScope;
        var collection = await CreateRandomCollectionAsync();

        //Act
        var updatedCollectionResponse = await client.PostAsJsonAsync(
            ApiRoutes.Collections.Create,
            new CreateCollectionItem()
            {
                CollectionId = short.Parse(collection.Id),
                Title = collectionName,
                IsDefaultBackSide = false,
                ThemeId = TestConstants.Theme.TestId,
            });
        var updatedCollection = updatedCollectionResponse.ToResponseDto<Collection>();


        //Assert
        updatedCollection.Should().NotBeNull();
        updatedCollection.Id.Should().Be(collection.Id);
        updatedCollection.Title.Should().BeEquivalentTo(collectionName);
    }
    
    [Theory]
    [InlineData("New collection")]
    [InlineData("My_Simple_Collection")]
    public async Task CreateCollection_ShouldActuallyUpdateCollection(string collectionName)
    {
        //Arrange
        var (client, scope) = SharedScope;
        var oldCollection = await CreateRandomCollectionAsync();
        var oldCollections = await GetAllCollectionsAsync();

        //Act
        await client.PostAsJsonAsync(
            ApiRoutes.Collections.Create,
            new CreateCollectionItem()
            {
                CollectionId = short.Parse(oldCollection.Id),
                Title = collectionName,
                IsDefaultBackSide = false,
                ThemeId = TestConstants.Theme.TestId,
            });

        //Assert
        var newCollections = await GetAllCollectionsAsync();
        oldCollections.Should().NotBeNull().And.HaveCount(1);
        newCollections.Should().NotBeNull().And.HaveCount(1);

        var newCollection = newCollections.Single();
        newCollection.Id.Should().Be(oldCollection.Id);
        newCollection.Title.Should().BeEquivalentTo(collectionName);
    }
    
    [Theory]
    [MemberData(nameof(IncorrectNames))]
    public async Task CreateCollection_ShouldFailOnUpdatingWithIncorrectName(string collectionName)
    {
        //Arrange
        var (client, scope) = SharedScope;
        var oldCollection = await CreateRandomCollectionAsync();

        //Act
        var updateResponse = await client.PostAsJsonAsync(
            ApiRoutes.Collections.Create,
            new CreateCollectionItem()
            {
                CollectionId = short.Parse(oldCollection.Id),
                Title = collectionName,
                IsDefaultBackSide = false,
                ThemeId = TestConstants.Theme.TestId,
            });

        //Assert
        updateResponse.IsSuccessStatusCode.Should().BeFalse();
    }
    
    [Theory]
    [MemberData(nameof(IncorrectNames))]
    public async Task CreateCollection_ShouldNotUpdateAnythingOnFail(string collectionName)
    {
        //Arrange
        var (client, scope) = SharedScope;
        var oldCollection = await CreateRandomCollectionAsync();
        var oldCollections = await GetAllCollectionsAsync();

        //Act
        await client.PostAsJsonAsync(
            ApiRoutes.Collections.Create,
            new CreateCollectionItem()
            {
                CollectionId = short.Parse(oldCollection.Id),
                Title = collectionName,
                IsDefaultBackSide = false,
                ThemeId = TestConstants.Theme.TestId,
            });

        //Assert
        var newCollections = await GetAllCollectionsAsync();
        oldCollections.Should().NotBeNull().And.HaveCount(1);
        newCollections.Should().NotBeNull().And.HaveCount(1);

        var newCollection = newCollections.Single();
        newCollection.Id.Should().Be(oldCollection.Id);
        newCollection.Title.Should().Be(oldCollection.Title);
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
    public async Task GetCollection_ShouldFailOnGettingUnknownCollection()
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
    public async Task SearchCollection_ShouldReturnSameValueForTheSamePage()
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

    [Fact]
    public async Task CreateCard_ShouldIncrementCounter()
    {
        //Arrange
        var (client, scope) = SharedScope;
        var oldCollection = await CreateRandomCollectionAsync();
        
        //Act
        var cardsCount = 10;
        await AddRandomCardsToCollection(oldCollection.Id, cardsCount);
        
        //Assert
        oldCollection.CardsCount.Should().Be(0);
        var newCollection = await GetCollectionAsync(oldCollection.Id);
        newCollection.CardsCount.Should().Be((short)cardsCount);
    }
    
    [Fact]
    public async Task CreateCard_ShouldDecrementCounter_WhenCardDeleted()
    {
        //Arrange
        var (client, scope) = SharedScope;
        var cardsCount = 10;
        var (collection, cards) = await CreateRandomCardsAsync(cardsCount);
        
        //Act
        await client.DeleteAsync(
            AbsoluteQuery(
                ApiRoutes.Cards.GetBasePath(short.Parse(collection.Id)),
                ApiRoutes.Cards.GetDeleteCardPath(short.Parse(cards.First().Id))));

        //Assert
        var newCollection = await GetCollectionAsync(collection.Id);
        newCollection.CardsCount.Should().Be((short)(cardsCount - 1));
    }
}