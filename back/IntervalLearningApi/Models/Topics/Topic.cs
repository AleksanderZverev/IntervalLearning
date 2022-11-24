using IntervalLearningApi.Models.ByUser;

public class Topic
{
    public long Id { get; set; }   
    public string Name { get; set; }
    public string Theory { get; set; }
    public long? ParentCourseId { get; set; }
    public List<Collection> CourseCollections { get; set; }
}