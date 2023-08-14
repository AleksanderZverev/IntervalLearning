using System.Net.Http.Json;
using DB;
using DB.Models;
using FluentAssertions;
using IntervalLearningApi.Constants;
using IntervalLearningApi.Controllers;
using IntervalLearningApi.IntegrationTests.Collections;
using IntervalLearningApi.IntegrationTests.Common;
using IntervalLearningApi.IntegrationTests.Common.Attributes;
using IntervalLearningApi.IntegrationTests.Common.Constants;
using IntervalLearningApi.IntegrationTests.Common.Extensions;
using IntervalLearningApi.Models.ByUser;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace IntervalLearningApi.IntegrationTests.Cards;

[UseDefaultTestUser]
[UseBasePath(ApiRoutes.Cards.BasePath)]
public class CardsControllerTests : BaseTests
{
    private IReadOnlyList<CardEntity> AllTestCards;
    private const int MaxCard = 30;
    private string CollectionId => TestConstants.Collection.Id.ToString();

    [OneTimeSetUp]
    public async Task SetUp()
    {
        using var scope = GetScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationContext>();
        
        var cards = new CardEntityFaker().Generate(MaxCard);

        cards.ForEach(c =>
        {
            c.ParentUserId = TestConstants.User.Id;
            c.ParentCollectionId = TestConstants.Collection.Id;
        });
        db.Cards.AddRange(cards);
        await db.SaveChangesAsync();
        
        AllTestCards = cards.ToList();
    }
    
    [Test, Order(1)]
    public async Task GetCards_ShouldReturnByPage()
    {
        var countPerPage = 5;
        
        var firstCardsPageResponse = await client.GetAsync(
            ApiRoutes.Cards.Get_GetAll +
            new QueryString()
                .Add("collectionId", CollectionId)
                .Add("page", "1")
                .Add("count", countPerPage.ToString()));
        var firstCardsPage = firstCardsPageResponse.ToResponseDto<List<Card>>();
        
        var secondCardsPageResponse = await client.GetAsync(
            ApiRoutes.Cards.Get_GetAll +
            new QueryString()
                .Add("collectionId", CollectionId)
                .Add("page", "2")
                .Add("count", countPerPage.ToString()));
        var secondCardsPage = secondCardsPageResponse.ToResponseDto<List<Card>>();

        firstCardsPage.Should().NotBeNull().And.NotBeEmpty();
        secondCardsPage.Should().NotBeNull().And.NotBeEmpty();
        var allCards = firstCardsPage.Union(secondCardsPage).ToList();
        allCards.Count.Should().Be(countPerPage * 2);
        allCards.Select(c => c.Id).Should().OnlyHaveUniqueItems();
        allCards.Select(c => c.Id).Should().BeSubsetOf(AllTestCards.Select(c => c.Id.ToString()));
    }
    
    [Test, Order(1)]
    public async Task GetCards_ShouldReturnSameCardsForSpecifiedPage()
    {
        var countPerPage = 5;
        
        var firstCardsPageResponse = await client.GetAsync(
            ApiRoutes.Cards.Get_GetAll +
            new QueryString()
                .Add("collectionId", CollectionId)
                .Add("page", "2")
                .Add("count", countPerPage.ToString()));
        var firstCardsPage = firstCardsPageResponse.ToResponseDto<List<Card>>();
        
        var secondCardsPageResponse = await client.GetAsync(
            ApiRoutes.Cards.Get_GetAll +
            new QueryString()
                .Add("collectionId", CollectionId)
                .Add("page", "2")
                .Add("count", countPerPage.ToString()));
        var secondCardsPage = secondCardsPageResponse.ToResponseDto<List<Card>>();

        firstCardsPage.Should().NotBeNull().And.NotBeEmpty();
        secondCardsPage.Should().NotBeNull().And.NotBeEmpty();
        firstCardsPage.Should().BeEquivalentTo(secondCardsPage);
    }
    
    [Test]
    public async Task CrateCard_ShouldCreateCard()
    {
        var fakeCard = new CardEntityFaker().Generate();
        
        var createdCard = await CreateCardAsync(fakeCard);

        createdCard.Should().NotBeNull();
        createdCard.Id.Should().NotBeEmpty();
        createdCard.FrontSideText.Should().Be(fakeCard.FrontSideText);
        createdCard.BackSideText.Should().Be(fakeCard.BackSideText);
        createdCard.PromptText.Should().Be(fakeCard.PromptText);
        createdCard.Description.Should().Be(fakeCard.Description);
        createdCard.Examples.Should().BeEquivalentTo(fakeCard.Examples);
    }

    [Test]
    public async Task DeleteCard_ShouldDelete()
    {
        var fakeCard = new CardEntityFaker().Generate();
        var createdCard = await CreateCardAsync(fakeCard);

        //TODO: use existing card?
        var deleteCardResponse = await client.DeleteAsync(
            ApiRoutes.Cards.GetDeleteCardPath(short.Parse(createdCard.Id)));
        var deletedCard = deleteCardResponse.ToResponseDto<Card>();

        deletedCard.Should().NotBeNull();
        deletedCard.Id.Should().Be(createdCard.Id);
    }
    
    [Test]
    public async Task MoveCard_ShouldMoveToOtherCollection()
    {
        var fakeCard = new CardEntityFaker().Generate();
        var createdCard = await CreateCardAsync(fakeCard);
        var otherCollectionId = TestConstants.Collection.Other.Id;

        var moveCardResponse = await client.PostAsJsonAsync(
            ApiRoutes.Cards.Post_MoveCard,
            new MoveRequest()
            {
                CardId = short.Parse(createdCard.Id),
                DestinationCollectionId = otherCollectionId,
            });
        var movedCard = moveCardResponse.ToResponseDto<Card>();

        movedCard.Should().NotBeNull();
        movedCard.ParentUserId.Should().Be(createdCard.ParentUserId);
        movedCard.ParentCollectionId.Should().Be(otherCollectionId.ToString());
    }

    [Test]
    public async Task Test()
    {
        
    }

    private async Task<Card?> CreateCardAsync(CardEntity card)
    {
        var createCardResponse = await client.PostAsJsonAsync(
            ApiRoutes.Cards.Post_CreateCard,
            new CreateCardItem()
            {
                BackText = card.BackSideText,
                FrontText = card.FrontSideText,
                PromptText = card.PromptText,
                Description = card.Description,
                Examples = card.Examples,
            });
        var createdCard = createCardResponse.ToResponseDto<Card>();
        return createdCard;
    }
}