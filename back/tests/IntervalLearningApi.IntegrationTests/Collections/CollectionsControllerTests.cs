using System.Net.Http.Json;
using DB;
using DB.Models;
using FluentAssertions;
using IntervalLearningApi.Constants;
using IntervalLearningApi.IntegrationTests.Common;
using IntervalLearningApi.IntegrationTests.Common.Attributes;
using IntervalLearningApi.IntegrationTests.Common.Constants;
using IntervalLearningApi.IntegrationTests.Common.Extensions;
using IntervalLearningApi.Models.ByUser;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace IntervalLearningApi.IntegrationTests.Collections;

[UseBasePath(ApiRoutes.Collections.BasePath)]
[UseDefaultTestUser]
public class CollectionsControllerTests : BaseTests
{
    private IReadOnlyList<CollectionEntity> AllTestCollections;
    private IReadOnlyDictionary<short, List<CardEntity>> collectionIdToCards;
    private const int MaxCollections = 10;
    private const int MaxCard = 20;

    [OneTimeSetUp]
    public async Task SetUp()
    {
        using var scope = GetScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationContext>();

        AllTestCollections = new CollectionEntityFaker().Generate(MaxCollections);

        await db.Collections.AddRangeAsync(AllTestCollections);
        await db.SaveChangesAsync();

        var collectionIdToCardsEditable = new Dictionary<short, List<CardEntity>>();
        collectionIdToCards = collectionIdToCardsEditable; 

        foreach (var collection in AllTestCollections)
        {
            var cards = new CardEntityFaker().Generate(MaxCard);
            cards.ForEach(c => c.ParentCollectionId = collection.Id);
            db.Cards.AddRange(cards);

            collectionIdToCardsEditable[collection.Id] = cards.ToList();
        }
        await db.SaveChangesAsync();
    }

    private async Task AddCollections(params string[] collectionNames)
    {
        using var scope = GetScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationContext>();

        foreach (var collectionName in collectionNames)
        {
            var collection = new CollectionEntityFaker().Generate();
            collection.Title = collectionName;
            await db.Collections.AddAsync(collection);
        }
        
        await db.SaveChangesAsync();
    }

    [Test, Order(1)]
    public async Task GetAll_ShouldReturnAllCollections()
    {
        var getAllResponse = await client.GetAsync(ApiRoutes.Collections.GetAll);
        var allCollections = getAllResponse.ToResponseDto<List<Collection>>();

        allCollections.Should().NotBeNull().And.NotBeEmpty();
        allCollections.Count.Should().Be(AllTestCollections.Count);
        allCollections.Select(c => c.Title).Should().BeEquivalentTo(AllTestCollections.Select(c => c.Title));
    }

    [Test]
    public async Task GetCollection_ShouldReturnExistingCollection()
    {
        foreach (var testCollection in AllTestCollections)
        {
            var getCollectionResponse = await client.GetAsync(ApiRoutes.Collections.GetCollectionPath(testCollection.Id));
            var collection = getCollectionResponse.ToResponseDto<Collection>();
            
            collection.Id.Should().Be(testCollection.Id.ToString());
            collection.Title.Should().Be(testCollection.Title);
            collection.ThemeId.Should().Be(testCollection.ThemeId);
            collection.IsPublic.Should().Be(testCollection.IsPublic);
            collection.CardsCount.Should().Be(testCollection.CardsCount);
        }
    }


    [TestCase("Mathematics", "mat")]
    [TestCase("Check_and_mate", "chec")]
    [TestCase("My collections", "My c")]
    public async Task SearchCollection_ShouldSearchByName(string collectionFullName, string searchRequestName)
    {
        await AddCollections(collectionFullName);
        
        var searchResponse = await client.GetAsync(
            ApiRoutes.Collections.SearchPrivate +
            new QueryString()
                .Add("themeId", TestConstants.Theme.TestId.ToString())
                .Add("searchName", searchRequestName));
        var searchResult = searchResponse.ToResponseDto<List<Collection>>();

        searchResult.Should().NotBeNull().And.NotBeEmpty();
        searchResult.Select(c => c.Title).Should().ContainSingle(collectionFullName);
    }

    [Test]
    public async Task SearchCollection_ShouldSearchByPages()
    {
        var countPerPage = 4;
        var fistPageResponse = await client.GetAsync(
            ApiRoutes.Collections.SearchPrivate +
            new QueryString()
                .Add("themeId", TestConstants.Theme.TestId.ToString())
                .Add("page", "1")
                .Add("count", countPerPage.ToString()));
        var firstPageCollections = fistPageResponse.ToResponseDto<List<Collection>>();

        var secondPageResponse = await client.GetAsync(
            ApiRoutes.Collections.SearchPrivate +
            new QueryString()
                .Add("themeId", TestConstants.Theme.TestId.ToString())
                .Add("page", "2")
                .Add("count", countPerPage.ToString()));
        var secondPageCollections = secondPageResponse.ToResponseDto<List<Collection>>();
        
        
        firstPageCollections.Should().NotBeNull().And.NotBeEmpty();
        secondPageCollections.Should().NotBeNull().And.NotBeEmpty();
        var allCollections = firstPageCollections.Union(secondPageCollections).ToList();

        allCollections.Select(c => c.Title).Should().OnlyHaveUniqueItems();
        allCollections.Select(c => c.Title).Should().BeSubsetOf(AllTestCollections.Select(c => c.Title));
    }

    [Test]
    public async Task SearchCollection_ShouldReturnSameValueForThePage()
    {
        var countPerPage = 5;
        var fistPageResponse = await client.GetAsync(
            ApiRoutes.Collections.SearchPrivate +
            new QueryString()
                .Add("themeId", TestConstants.Theme.TestId.ToString())
                .Add("page", "2")
                .Add("count", countPerPage.ToString()));
        var firstPageCollections = fistPageResponse.ToResponseDto<List<Collection>>();

        var secondPageResponse = await client.GetAsync(
            ApiRoutes.Collections.SearchPrivate +
            new QueryString()
                .Add("themeId", TestConstants.Theme.TestId.ToString())
                .Add("page", "2")
                .Add("count", countPerPage.ToString()));
        var secondPageCollections = secondPageResponse.ToResponseDto<List<Collection>>();

        firstPageCollections.Should().NotBeNull().And.NotBeEmpty();
        secondPageCollections.Should().NotBeNull().And.NotBeEmpty();
        firstPageCollections.Should().BeEquivalentTo(secondPageCollections);
        
        firstPageCollections.Select(c => c.Title).Should().BeSubsetOf(AllTestCollections.Select(c => c.Title));
    }

    [TestCase("New collection")]
    public async Task CreateCollection_ShouldCreate(string collectionName)
    {
        var createResponse = await client.PostAsJsonAsync(ApiRoutes.Collections.Create, new CreateCollectionItem()
        {
            Title = collectionName,
            IsDefaultBackSide = false,
            ThemeId = TestConstants.Theme.TestId,
        });

        createResponse.IsSuccessStatusCode.Should().BeTrue();
        var response = createResponse.ToResponseDto<Collection>();

        response.Should().NotBeNull();
        response.Title.Should().BeEquivalentTo(collectionName);
    }
}