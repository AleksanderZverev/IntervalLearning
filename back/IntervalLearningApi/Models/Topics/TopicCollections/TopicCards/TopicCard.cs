namespace IntervalLearningApi.Models.Topics.TopicCollections.TopicCards;

public class TopicCard
{
    public long Id { get; set; }
    public string RememberingText { get; set; }
    public string PromptText { get; set; }
    public string MeaningText { get; set; }
    public string? Description { get; set; }
    public List<string>? Examples { get; set; }

    public long ParentCourseId { get; set; }
    public long ParentTopicId { get; set; }
    public long ParentTopicCollectionId { get; set; }
}