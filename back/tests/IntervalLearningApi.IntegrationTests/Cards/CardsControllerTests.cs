using Bogus;
using IntervalLearningApi.IntegrationTests.Common.Fakers.Api;
using IntervalLearningApi.Models.ByUser;

namespace IntervalLearningApi.IntegrationTests.Cards;

public class CardsControllerTests : SharedApiTests
{
    public CardsControllerTests(SharedDockerIntervalLearningApiFactory apiFactory) : base(apiFactory)
    {
    }
    
    private string Query(string collectionId, string path)
        => AbsoluteQuery(ApiRoutes.Cards.GetBasePath(short.Parse(collectionId)), path);

    private async Task<List<Card>?> GetCardsPageAsync(
        HttpClient client,
        CollectionDto collection,
        int pageNumber,
        int countPerPage)
    {
        var pageCardsResponse = await client.GetAsync(
            Query(collection.Id, ApiRoutes.Cards.Get_GetAll) +
            new QueryString()
                .Add("collectionId", collection.Id)
                .Add("page", pageNumber.ToString())
                .Add("count", countPerPage.ToString()));
        var pageCards = pageCardsResponse.ToResponseDto<List<Card>>();
        return pageCards;
    }

    [Fact]
    public async Task GetCards_ShouldReturnEmpty_IfNoCardsAdded()
    {
        //Arrange
        var (client, user) = SharedScope;
        var collection = await CreateRandomCollectionAsync();

        //Act
        var pageCards = await GetCardsPageAsync(client, collection, 1, 50);

        //Assert
        pageCards.Should().NotBeNull().And.BeEmpty();
    }

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
            var pageCards = await GetCardsPageAsync(client, collection, pageNumber, countPerPage);
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

    [Fact]
    public async Task GetCards_ShouldReturnSameCardsForSpecifiedPage()
    {
        //Arrange
        var (client, user) = SharedScope;
        var pages = 6;
        var countPerPage = 4;
        var (collection, preAddedCards) = await CreateRandomCardsAsync(pages * countPerPage);

        //Act
        var pageNumber = Random.Shared.Next(1, pages + 1);
        var firstCardsPage = await GetCardsPageAsync(client, collection, pageNumber, countPerPage);
        var secondCardsPage = await GetCardsPageAsync(client, collection, pageNumber, countPerPage);

        //Assert
        firstCardsPage.Should().NotBeNullOrEmpty();
        secondCardsPage.Should().NotBeNullOrEmpty();

        firstCardsPage.Select(c => c.Id).Should().Equal(secondCardsPage.Select(c => c.Id));
        firstCardsPage.Select(c => c.FrontSideText).Should().Equal(secondCardsPage.Select(c => c.FrontSideText));

        firstCardsPage.Select(c => c.Id).Should().BeSubsetOf(preAddedCards.Select(c => c.Id));
    }
    
    public static IEnumerable<object[]> IncorrectSearchRequests = new object[][]
    {
        new[] { new string('a', 300) },
    };
    
    [Fact]
    public async Task CreateCard_ShouldCreateCard_WithFullData()
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
    public async Task CreateCard_ShouldCreateCardWithMinimalData()
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
                FrontText = fakeCard.FrontText,
            });

        //Assert
        createdCard.Should().NotBeNull();
        createdCard.Id.Should().NotBeNullOrEmpty();
        createdCard.FrontSideText.Should().Be(fakeCard.FrontText);
        createdCard.BackSideText.Should().Be(fakeCard.BackText);
        createdCard.PromptText.Should().BeNullOrEmpty();
        createdCard.Description.Should().BeNullOrEmpty();
        createdCard.Examples.Should().BeNullOrEmpty();
    }
    
    [Fact]
    public async Task CreateCard_ShouldReturnUpdatedValue()
    {
        //Arrange
        var (client, user) = SharedScope;
        var (collection, oldCard) = await CreateRandomCardAsync();

        //Act
        var fakeCard = new CardFaker().Generate();
        var updateItem = new CreateCardItem()
        {
            CardId = short.Parse(oldCard.Id),
            BackText = fakeCard.BackText,
            FrontText = fakeCard.FrontText,
        };
        var updatedCard = await CreateCardAsync(short.Parse(collection.Id), updateItem);

        //Assert
        updatedCard.Should().NotBeNull();
        updatedCard.Id.Should().Be(oldCard.Id);
        updatedCard.FrontSideText.Should().Be(updatedCard.FrontSideText);
        updatedCard.BackSideText.Should().Be(updatedCard.BackSideText);
        updatedCard.PromptText.Should().BeNullOrEmpty();
        updatedCard.Description.Should().BeNullOrEmpty();
        updatedCard.Examples.Should().BeNullOrEmpty();
    }
    
    [Fact]
    public async Task CreateCard_ShouldActuallyUpdateCard()
    {
        //Arrange
        var (client, user) = SharedScope;
        var (collection, oldCard) = await CreateRandomCardAsync();

        //Act
        var fakeCard = new CardFaker().Generate();
        var updateItem = new CreateCardItem()
        {
            CardId = short.Parse(oldCard.Id),
            BackText = fakeCard.BackText,
            FrontText = fakeCard.FrontText,
        };
        await CreateCardAsync(short.Parse(collection.Id), updateItem);

        //Assert
        var cards = await GetCardsPageAsync(client, collection, 1, 50);
        cards.Should().HaveCount(1);
        
        var updatedCardFromList = cards.Single();
        updatedCardFromList.Id.Should().Be(oldCard.Id);
        updatedCardFromList.FrontSideText.Should().Be(updateItem.FrontText);
        updatedCardFromList.BackSideText.Should().Be(updateItem.BackText);
        updatedCardFromList.PromptText.Should().BeNullOrEmpty();
        updatedCardFromList.Description.Should().BeNullOrEmpty();
        updatedCardFromList.Examples.Should().BeNullOrEmpty();
    }
    
    [Fact]
    public async Task CreateCard_ShouldFailOnUpdatingUnknownCard()
    {
        //Arrange
        var (client, user) = SharedScope;
        var (collection, card) = await CreateRandomCardAsync();

        //Act
        var fakeCard = new CardFaker().Generate();
        var updatedCard = await CreateCardAsync(
            short.Parse(collection.Id), 
            new CreateCardItem()
        {
            CardId = (short)(short.Parse(card.Id) + 1),
            BackText = fakeCard.BackText,
            FrontText = fakeCard.FrontText,
        });

        //Assert
        updatedCard.Should().BeNull();
    }
    
    public static IEnumerable<object[]> IncorrectCardsData = new object[][]
    {
        new[] { "", "", null, null },
        
        new[] { "back text", "", null, null },
        new[] { "", "front text", null, null },
        
        new[] { new string('a', 300), "front text", null, null },
        new[] { "back text", new string('a', 300), null, null},
        
        new[] { "back text", "front text", new string('a', 600), null },
        
        new object[] { "back text", "front text", new string('a', 200), new[] { new string('a', 400) } },
    };
    
    [Theory]
    [MemberData(nameof(IncorrectCardsData))]
    public async Task CreateCard_ShouldFailOnIncorrectData(
        string backText,
        string frontText,
        string? description,
        string[]? examples)
    {
        //Arrange
        var (client, user) = SharedScope;
        var collection = await CreateRandomCollectionAsync();
        
        //Act
        var createdCard = await CreateCardAsync(
            short.Parse(collection.Id),
            new CreateCardItem()
            {
                BackText = backText,
                FrontText = frontText,
                Description = description,
                Examples = examples?.ToList(),
            });

        //Assert
        createdCard.Should().BeNull();
    }
    
    [Fact]
    public async Task DeleteCard_ShouldReturnDeletedCardData()
    {
        //Arrange
        var (client, user) = SharedScope;
        var (collection, createdCard) = await CreateRandomCardAsync();

        //Act
        var deleteCardResponse = await client.DeleteAsync(
            Query(collection.Id, ApiRoutes.Cards.GetDeleteCardPath(short.Parse(createdCard.Id))));
        var deletedCard = deleteCardResponse.ToResponseDto<Card>();

        //Assert
        deletedCard.Should().NotBeNull();
        deletedCard.Id.Should().Be(createdCard.Id);
        deletedCard.FrontSideText.Should().BeEquivalentTo(createdCard.FrontSideText);
        deletedCard.BackSideText.Should().BeEquivalentTo(createdCard.BackSideText);
        deletedCard.PromptText.Should().BeEquivalentTo(createdCard.PromptText);
        deletedCard.Description.Should().BeEquivalentTo(createdCard.Description);
        deletedCard.Examples.Should().BeEquivalentTo(createdCard.Examples);
    }

    [Fact]
    public async Task DeleteCard_ShouldActuallyDeleteExistingCard()
    {
        //Arrange
        var (client, user) = SharedScope;
        var (collection, createdCard) = await CreateRandomCardAsync();
        var oldPage = await GetCardsPageAsync(client, collection, 1, 50);

        //Act
        var deleteCardResponse = await client.DeleteAsync(
            Query(collection.Id, ApiRoutes.Cards.GetDeleteCardPath(short.Parse(createdCard.Id))));
        var deletedCard = deleteCardResponse.ToResponseDto<Card>();

        //Assert
        deletedCard.Should().NotBeNull();

        oldPage.Should().NotBeEmpty();
        
        var newPage = await GetCardsPageAsync(client, collection, 1, 50);
        newPage.Should().BeEmpty();
    }
    
    [Fact]
    public async Task DeleteCard_ShouldFailOnDeletingUnknownCard()
    {
        //Arrange
        var (client, user) = SharedScope;
        var (collection, createdCard) = await CreateRandomCardAsync();

        //Act
        var randomCardId = new Faker().Random.Short(min: (short)(short.Parse(createdCard.Id) + 1));
        var deleteCardResponse = await client.DeleteAsync(
            Query(collection.Id, ApiRoutes.Cards.GetDeleteCardPath(randomCardId)));

        //Assert
        deleteCardResponse.IsSuccessStatusCode.Should().BeFalse();
    }
    
    [Fact]
    public async Task DeleteCard_ShouldNotDeleteAnythingOnFail()
    {
        //Arrange
        var (client, user) = SharedScope;
        var (collection, createdCard) = await CreateRandomCardAsync();
        var oldPage = await GetCardsPageAsync(client, collection, 1, 50);

        //Act
        var randomCardId = new Faker().Random.Short(min: (short)(short.Parse(createdCard.Id) + 1));
        await client.DeleteAsync(Query(
            collection.Id,
            ApiRoutes.Cards.GetDeleteCardPath(randomCardId)));

        //Assert
        var newPage = await GetCardsPageAsync(client, collection, 1, 50);
        oldPage.Count.Should().Be(newPage.Count);
        oldPage.Select(c => c.Id).Should().Equal(newPage.Select(c => c.Id));
    }
    
    [Fact]
    public async Task MoveCard_ShouldReturnNewCollectionData()
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

        //Assert
        movedCard.Should().NotBeNull();
        movedCard.ParentUserId.Should().Be(createdCard.ParentUserId);
        movedCard.ParentCollectionId.Should().Be(otherCollection.Id);
    }
    
    [Fact]
    public async Task MoveCard_ShouldActuallyMoveToNewCollection()
    {
        //Arrange
        var (client, user) = SharedScope;
        var (collection, createdCard) = await CreateRandomCardAsync();
        var otherCollection = await CreateRandomCollectionAsync();
        
        var oldCollectionPage = await GetCardsPageAsync(client, collection, 1, 50);
        var oldOtherCollectionPage = await GetCardsPageAsync(client, otherCollection, 1, 50);
        
        //Act
        await client.PostAsJsonAsync(
            Query(collection.Id, ApiRoutes.Cards.Post_MoveCard),
            new MoveRequest()
            {
                CardId = short.Parse(createdCard.Id),
                DestinationCollectionId = short.Parse(otherCollection.Id),
            });

        //Assert
        oldCollectionPage.Should().NotBeEmpty();
        var newCollectionPage = await GetCardsPageAsync(client, collection, 1, 50);
        newCollectionPage.Should().BeEmpty();

        oldOtherCollectionPage.Should().BeEmpty();
        var newOtherCollectionPage = await GetCardsPageAsync(client, otherCollection, 1, 50);
        newOtherCollectionPage.Should().NotBeEmpty();
    }
    
    [Fact]
    public async Task MoveCard_ShouldFailOnMoveToUnknownCollection()
    {
        //Arrange
        var (client, user) = SharedScope;
        var (collection, createdCard) = await CreateRandomCardAsync();
        
        //Act
        var randomCollectionId = new Faker().Random.Short(min: (short)(short.Parse(collection.Id) + 1));
        var moveCardResponse = await client.PostAsJsonAsync(
            Query(collection.Id, ApiRoutes.Cards.Post_MoveCard),
            new MoveRequest()
            {
                CardId = short.Parse(createdCard.Id),
                DestinationCollectionId = randomCollectionId,
            });
        
        //Assert
        moveCardResponse.IsSuccessStatusCode.Should().BeFalse();
    }
    
    [Fact]
    public async Task MoveCard_ShouldNotMoveAnythingOnFail()
    {
        //Arrange
        var (client, user) = SharedScope;
        var (collection, createdCard) = await CreateRandomCardAsync();
        var otherCollection = await CreateRandomCollectionAsync();
        
        var oldCollectionPage = await GetCardsPageAsync(client, collection, 1, 50);
        var oldOtherCollectionPage = await GetCardsPageAsync(client, otherCollection, 1, 50);
        
        //Act
        var randomCollectionId = new Faker().Random.Short(min: (short)(short.Parse(collection.Id) + 1));
        await client.PostAsJsonAsync(
            Query(collection.Id, ApiRoutes.Cards.Post_MoveCard),
            new MoveRequest()
            {
                CardId = short.Parse(createdCard.Id),
                DestinationCollectionId = randomCollectionId,
            });
        
        //Assert
        var newCollectionPage = await GetCardsPageAsync(client, collection, 1, 50);
        oldCollectionPage.Should().BeEquivalentTo(newCollectionPage);

        var newOtherCollectionPage = await GetCardsPageAsync(client, otherCollection, 1, 50);
        oldOtherCollectionPage.Should().BeEquivalentTo(newOtherCollectionPage);
    }


    public static string[] OneWordSentences =
    {
        "hello",
        "world",
        "there",
        "wonderful"
    };
    
    public static string[] ManyWordsSentences =
    {
        "see you again",
        "weather is nice today",
        "see the new film",
    };

    public static IEnumerable<object[]> SearchValues = new object[][]
    {
        //with one words
        new object[] { "he", OneWordSentences, new[] { "hello" } },
        new object[] { "wo", OneWordSentences, new[] { "world", "wonderful" } },

        //with many words
        new object[] { "see", ManyWordsSentences, new[] { "see you again", "see the new film" } },
        new object[] { "w", ManyWordsSentences, new[] { "weather is nice today" } },

        //sub strings (not implemented)
        // new object[] { "yo", ManyWordsSentences, new[] { "see you again" } },
        // new object[] { "ni", ManyWordsSentences, new[] { "weather is nice today" } },
    };
    
    
    [Theory]
    [MemberData(nameof(SearchValues))]
    public async Task SearchCard_ShouldSearchByDifferentSubstrings(
        string searchInput,
        string[] fieldTypePossibleValues,
        string[] shouldContainValues)
    {
        //Arrange
        var (client, user) = SharedScope;
        var collection = await CreateRandomCollectionAsync();
        var cards = new List<Card>(fieldTypePossibleValues.Length);
        foreach (var fieldValue in fieldTypePossibleValues)
        {
            var fakeCardData = new CardFaker().Generate();
            fakeCardData.FrontText = fieldValue;
            
            var createdCard = await CreateCardAsync(
                short.Parse(collection.Id), fakeCardData);

            if (createdCard == null)
                throw new InvalidOperationException("Unable to create random card");
            
            cards.Add(createdCard);
        }
        
        //Act
        var searchResponse = await client.GetAsync(
            Query(collection.Id, ApiRoutes.Cards.Get_SearchCard) +
            new QueryString()
                .Add("searchValue", searchInput)
                .Add("fieldType", SearchFieldType.RememberingText.ToString("G")));
        var searchResult = searchResponse.ToResponseDto<List<Card>>();

        //Assert
        searchResult.Should().NotBeNullOrEmpty();
        searchResult.Select(c => c.FrontSideText).Should().BeEquivalentTo(shouldContainValues);
    }
    
    public static IEnumerable<object[]> SearchValuesByTypes = new object[][]
    {
        new object[] { "see", SearchFieldType.RememberingText, ManyWordsSentences},
        new object[] { "see", SearchFieldType.PromptText, ManyWordsSentences},
        new object[] { "see", SearchFieldType.MeaningText, ManyWordsSentences},
    };
    
    [Theory]
    [MemberData(nameof(SearchValuesByTypes))]
    public async Task SearchCard_ShouldSearchByEveryTypeOfFields(
        string searchInput,
        SearchFieldType searchFieldType,
        string[] fieldTypePossibleValues)
    {
        //Arrange
        var (client, user) = SharedScope;
        var collection = await CreateRandomCollectionAsync();
        var cards = new List<Card>(fieldTypePossibleValues.Length);
        foreach (var fieldValue in fieldTypePossibleValues)
        {
            var fakeCardData = new CardFaker().Generate();

            switch (searchFieldType)
            {
                case SearchFieldType.RememberingText:
                {
                    fakeCardData.FrontText = fieldValue;
                    break;
                }
                case SearchFieldType.MeaningText:
                {
                    fakeCardData.BackText = fieldValue;
                    break;
                }
                case SearchFieldType.PromptText:
                {
                    fakeCardData.PromptText = fieldValue;
                    break;
                }
                default: throw new NotImplementedException("Unknown type of search field");
            }
            
            var createdCard = await CreateCardAsync(
                short.Parse(collection.Id), fakeCardData);

            if (createdCard == null)
                throw new InvalidOperationException("Unable to create random card");
            
            cards.Add(createdCard);
        }
        
        //Act
        var searchResponse = await client.GetAsync(
            Query(collection.Id, ApiRoutes.Cards.Get_SearchCard) +
            new QueryString()
                .Add("searchValue", searchInput)
                .Add("fieldType", searchFieldType.ToString("G")));
        var searchResult = searchResponse.ToResponseDto<List<Card>>();

        //Assert
        searchResult.Should().NotBeNullOrEmpty();
        searchResult.Select(c =>
        {
            return searchFieldType switch
            {
                SearchFieldType.RememberingText => c.FrontSideText,
                SearchFieldType.MeaningText => c.BackSideText,
                SearchFieldType.PromptText => c.PromptText,
                _ => throw new NotImplementedException("Unknown type of search field"),
            };
        }).Should().BeSubsetOf(fieldTypePossibleValues);
    }
    
    public static IEnumerable<object[]> SearchByPageValues = new object[][]
    {
        //In the same range
        new object[] { "a", 2, 1, new[] { "aaa", "abb", "ccc", "dddd"}, new[] {"aaa", "abb"}},
        
        //In different ranges
        new object[] { "ab", 2, 1, new[] { "aaa", "abb", "ccc", "abd",}, new[] {"abb", "abd"}},
    };
    
    [Theory]
    [MemberData(nameof(SearchByPageValues))]
    public async Task SearchCard_ShouldSearchByPages(
        string searchInput,
        int pages,
        int countPerPage,
        string[] possibleValues,
        string[] shouldContainValues)
    {
        //Arrange
        var (client, user) = SharedScope;
        var collection = await CreateRandomCollectionAsync();
        var preAddedCards = new List<Card>(possibleValues.Length);
        foreach (var fieldValue in possibleValues)
        {
            var fakeCardData = new CardFaker().Generate();
            fakeCardData.FrontText = fieldValue;
            
            var createdCard = await CreateCardAsync(
                short.Parse(collection.Id), fakeCardData);

            if (createdCard == null)
                throw new InvalidOperationException("Unable to create random card");
            
            preAddedCards.Add(createdCard);
        }
        
        //Act
        var pageToCards = new List<(int Page, List<Card>? Cards)>();
        for (var pageNumber = 1; pageNumber <= pages; pageNumber++)
        {
            var searchResponse = await client.GetAsync(
                Query(collection.Id, ApiRoutes.Cards.Get_SearchCard) +
                new QueryString()
                    .Add("searchValue", searchInput)
                    .Add("fieldType", SearchFieldType.RememberingText.ToString("G"))
                    .Add("page", pageNumber.ToString())
                    .Add("count", countPerPage.ToString()));
            var searchResult = searchResponse.ToResponseDto<List<Card>>();

            pageToCards.Add((pageNumber, searchResult));
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
        allCards.Select(c => c.FrontSideText).Should().BeSubsetOf(shouldContainValues);
    }
    
    public static IEnumerable<object[]> SearchBySamePageValues = new object[][]
    {
        //In the same range
        new object[] { "a", 2, 1, new[] { "aaa", "abb", "ccc", "dddd"}},
        
        //In different ranges
        new object[] { "ab", 2, 1, new[] { "aaa", "abb", "ccc", "abd",}},
    };
    
    [Theory]
    [MemberData(nameof(SearchBySamePageValues))]
    public async Task SearchCard_ShouldSearchSameResponseOnTheSamePage(
        string searchInput,
        int pages,
        int countPerPage,
        string[] possibleValues)
    {
        //Arrange
        var (client, user) = SharedScope;
        var collection = await CreateRandomCollectionAsync();
        var preAddedCards = new List<Card>(possibleValues.Length);
        foreach (var fieldValue in possibleValues)
        {
            var fakeCardData = new CardFaker().Generate();
            fakeCardData.FrontText = fieldValue;
            
            var createdCard = await CreateCardAsync(
                short.Parse(collection.Id), fakeCardData);

            if (createdCard == null)
                throw new InvalidOperationException("Unable to create random card");
            
            preAddedCards.Add(createdCard);
        }
        
        //Act
        var randomPageNumber = Random.Shared.Next(1, pages + 1);
        var firstCardsPageResponse = await client.GetAsync(
            Query(collection.Id, ApiRoutes.Cards.Get_SearchCard) +
            new QueryString()
                .Add("searchValue", searchInput)
                .Add("fieldType", SearchFieldType.RememberingText.ToString("G"))
                .Add("page", randomPageNumber.ToString())
                .Add("count", countPerPage.ToString()));
        var firstCardsPage = firstCardsPageResponse.ToResponseDto<List<Card>>();
        
        var secondCardsPageResponse = await client.GetAsync(
            Query(collection.Id, ApiRoutes.Cards.Get_SearchCard) +
            new QueryString()
                .Add("searchValue", searchInput)
                .Add("fieldType", SearchFieldType.RememberingText.ToString("G"))
                .Add("page", randomPageNumber.ToString())
                .Add("count", countPerPage.ToString()));
        var secondCardsPage = secondCardsPageResponse.ToResponseDto<List<Card>>();
        
        //Assert
        firstCardsPage.Should().NotBeNullOrEmpty();
        secondCardsPage.Should().NotBeNullOrEmpty();

        firstCardsPage.Select(c => c.Id).Should().Equal(secondCardsPage.Select(c => c.Id));
        firstCardsPage.Select(c => c.FrontSideText).Should().Equal(secondCardsPage.Select(c => c.FrontSideText));

        firstCardsPage.Select(c => c.Id).Should().BeSubsetOf(preAddedCards.Select(c => c.Id));
    }
    
    public static IEnumerable<object[]> SearchValuesWithEmptyResponse = new object[][]
    {
        //with no words
        new object[] { "he", new string[] { } },
        
        //with one words
        new object[] { "he", new[] { "world", "wonderful" } },

        //with many words
        new object[] { "wh", new[] { "see you again", "see the new film" } },

        //sub strings (not implemented)
        // new object[] { "we", ManyWordsSentences, new[] { "see you again" } },
    };
    
    [Theory]
    [MemberData(nameof(SearchValuesWithEmptyResponse))]
    public async Task SearchCard_ShouldReturnEmptyList_WhenNothingFound(
        string searchInput,
        string[] fieldTypePossibleValues)
    {
        //Arrange
        var (client, user) = SharedScope;
        var collection = await CreateRandomCollectionAsync();
        var cards = new List<Card>(fieldTypePossibleValues.Length);
        foreach (var fieldValue in fieldTypePossibleValues)
        {
            var fakeCardData = new CardFaker().Generate();
            fakeCardData.FrontText = fieldValue;
            
            var createdCard = await CreateCardAsync(
                short.Parse(collection.Id), fakeCardData);

            if (createdCard == null)
                throw new InvalidOperationException("Unable to create random card");
            
            cards.Add(createdCard);
        }
        
        //Act
        var searchResponse = await client.GetAsync(
            Query(collection.Id, ApiRoutes.Cards.Get_SearchCard) +
            new QueryString()
                .Add("searchValue", searchInput)
                .Add("fieldType", SearchFieldType.RememberingText.ToString("G")));
        var searchResult = searchResponse.ToResponseDto<List<Card>>();

        //Assert
        searchResult.Should().BeEmpty();
    }

    public async Task Method_Should()
    {
        
    }
}