using IntervalLearningApi.IntegrationTests.Common.Fakers.Api;
using IntervalLearningApi.Models.ByUser;

namespace IntervalLearningApi.IntegrationTests.Cards;

public class CardsControllerTests : SharedApiTests
{
    public CardsControllerTests(IntervalLearningApiFactory apiFactory) : base(apiFactory)
    {
    }
    
    private string Query(string collectionId, string path)
        => AbsoluteQuery(ApiRoutes.Cards.GetBasePath(short.Parse(collectionId)), path);
    
    [Fact]
    public async Task GetCards_ShouldReturnByPage()
    {
        //Arrange
        var (client, user) = SharedScope;
        var pages = 6;
        var countPerPage = 4;
        var (collection, preAddedCards) = await CreateRandomCardsAsync(pages * countPerPage);

        //Act
        var pageToCards = new List<(int Page, List<Card>? Cards)>(pages);
        for (var i = 0; i < pages; i++)
        {
            var pageNumber = i + 1;
            var pageCardsResponse = await client.GetAsync(
                Query(collection.Id, ApiRoutes.Cards.Get_GetAll) +
                new QueryString()
                    .Add("collectionId", collection.Id)
                    .Add("page", pageNumber.ToString())
                    .Add("count", countPerPage.ToString()));
            var pageCards = pageCardsResponse.ToResponseDto<List<Card>>();
            pageToCards.Add((pageNumber, pageCards));
        }

        //Assert
        pageToCards.Count.Should().Be(pages);
        pageToCards.Select(c => c.Cards).Should().AllSatisfy(c =>
        {
            c.Should().NotBeNullOrEmpty();
            c.Count.Should().Be(countPerPage);
        });
        
        var allCards = pageToCards.SelectMany(c => c.Cards).ToList();
        allCards.Select(c => c.Id).Should().OnlyHaveUniqueItems();
        allCards.Select(c => c.Id).Should().BeSubsetOf(preAddedCards.Select(c => c.Id.ToString()));
        allCards.Select(c => c.FrontSideText).Should().BeSubsetOf(preAddedCards.Select(c => c.FrontSideText));
    }
    
    // [Test, Order(1)]
    [Fact]
    public async Task GetCards_ShouldReturnSameCardsForSpecifiedPage()
    {
        //Arrange
        var (client, user) = SharedScope;
        var pages = 6;
        var countPerPage = 4;
        var (collection, preAddedCards) = await CreateRandomCardsAsync(pages * countPerPage);

        //Act
        var pageNumber = Random.Shared.Next(0, pages + 1);
        var firstCardsPageResponse = await client.GetAsync(
            Query(collection.Id, ApiRoutes.Cards.Get_GetAll) +
            new QueryString()
                .Add("collectionId", collection.Id)
                .Add("page", pageNumber.ToString())
                .Add("count", countPerPage.ToString()));
        var firstCardsPage = firstCardsPageResponse.ToResponseDto<List<Card>>();
        
        var secondCardsPageResponse = await client.GetAsync(
            Query(collection.Id, ApiRoutes.Cards.Get_GetAll) +
            new QueryString()
                .Add("collectionId", collection.Id)
                .Add("page", pageNumber.ToString())
                .Add("count", countPerPage.ToString()));
        var secondCardsPage = secondCardsPageResponse.ToResponseDto<List<Card>>();

        //Assert
        firstCardsPage.Should().NotBeNullOrEmpty();
        secondCardsPage.Should().NotBeNullOrEmpty();

        firstCardsPage.Select(c => c.Id).Should().Equal(secondCardsPage.Select(c => c.Id));
        firstCardsPage.Select(c => c.FrontSideText).Should().Equal(secondCardsPage.Select(c => c.FrontSideText));

        firstCardsPage.Select(c => c.Id).Should().BeSubsetOf(preAddedCards.Select(c => c.Id));
    }
    
    [Fact]
    public async Task CreateCard_ShouldCreateCard()
    {
        //Arrange
        var (client, user) = SharedScope;
        var collection = await CreateRandomCollectionAsync();
        
        //Act
        var fakeCard = new CardFaker().Generate();
        var createdCard = await CreateCardAsync(
            short.Parse(collection.Id),
            new CreateCardItem()
            {
                BackText = fakeCard.BackText,
                PromptText = fakeCard.PromptText,
                FrontText = fakeCard.FrontText,
                Description = fakeCard.Description,
                Examples = fakeCard.Examples,
            });

        //Assert
        createdCard.Should().NotBeNull();
        createdCard.Id.Should().NotBeNullOrEmpty();
        createdCard.FrontSideText.Should().Be(fakeCard.FrontText);
        createdCard.BackSideText.Should().Be(fakeCard.BackText);
        createdCard.PromptText.Should().Be(fakeCard.PromptText);
        createdCard.Description.Should().Be(fakeCard.Description);
        createdCard.Examples.Should().BeEquivalentTo(fakeCard.Examples);
    }

    [Fact]
    public async Task DeleteCard_ShouldDelete()
    {
        //Arrange
        var (client, user) = SharedScope;
        var (collection, createdCard) = await CreateRandomCardAsync();
        
        //Act
        var deleteCardResponse = await client.DeleteAsync(
            Query(collection.Id, ApiRoutes.Cards.GetDeleteCardPath(short.Parse(createdCard.Id))));
        var deletedCard = deleteCardResponse.ToResponseDto<Card>();

        deletedCard.Should().NotBeNull();
        deletedCard.Id.Should().Be(createdCard.Id);
        //TODO: Check list of cards
    }
    
    [Fact]
    public async Task MoveCard_ShouldMoveToOtherCollection()
    {
        //Arrange
        var (client, user) = SharedScope;
        var (collection, createdCard) = await CreateRandomCardAsync();
        var otherCollection = await CreateRandomCollectionAsync();
        
        //Act
        var moveCardResponse = await client.PostAsJsonAsync(
            Query(collection.Id, ApiRoutes.Cards.Post_MoveCard),
            new MoveRequest()
            {
                CardId = short.Parse(createdCard.Id),
                DestinationCollectionId = short.Parse(otherCollection.Id),
            });
        var movedCard = moveCardResponse.ToResponseDto<Card>();

        movedCard.Should().NotBeNull();
        movedCard.ParentUserId.Should().Be(createdCard.ParentUserId);
        movedCard.ParentCollectionId.Should().Be(otherCollection.Id);
    }
    
    public async Task Method_Should()
    {
        
    }
}