using DB.Models;

namespace IntervalLearningApi.Models.Topics;

public class Topic
{
    public long Id { get; set; }
    public long ParentCourseId { get; set; }
    public string Name { get; set; }
    public List<CardEntity> CourseCollections { get; set; }
    public string Theory { get; set; }
}