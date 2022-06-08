namespace IntervalLearningApi.Models.Store;

public class PublicCollection
{
    public string OwnerUserId { get; }
    public string Id { get; }

    public string Title { get; }
    public string ShortDescription { get; }

    public short ThemeId { get; }
    public DateOnly PublishDate { get; }

    public short CardsCount { get; }


    public uint SubscribersCount { get; }
    public uint LikesCount { get; }
    public uint DislikesCount { get; }

    public PublicCollection(
        long ownerUserId,
        short id,
        string title,
        string shortDescription,
        short themeId,
        DateOnly publishDate,
        short cardsCount,
        uint subscribersCount,
        uint likesCount,
        uint dislikesCount)
    {
        Id = id.ToString();
        Title = title;
        ShortDescription = shortDescription;
        ThemeId = themeId;
        PublishDate = publishDate;
        CardsCount = cardsCount;
        OwnerUserId = ownerUserId.ToString();
        SubscribersCount = subscribersCount;
        LikesCount = likesCount;
        DislikesCount = dislikesCount;
    }
}