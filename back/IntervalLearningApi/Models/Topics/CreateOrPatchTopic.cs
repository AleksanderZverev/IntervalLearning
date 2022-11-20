namespace IntervalLearningApi.Models.Topics;

public class CreateOrPatchTopic
{
    public Guid Id { get; set; }
    public Guid ParentCourseId { get; set; }
    public string Name { get; set; }
    public string Theory { get; set; }
}