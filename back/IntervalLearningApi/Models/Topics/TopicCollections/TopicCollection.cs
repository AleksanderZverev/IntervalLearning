namespace IntervalLearningApi.Models.Topics.TopicCollections;

public class TopicCollection
{
    public long Id { get; set; }
    public string Name { get; set; }
    public long ParentCourseId { get; set; }
    public long ParentTopicId { get; set; }
    public List<TopicCard> Cards { get; set; } = new();
}