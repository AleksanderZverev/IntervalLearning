namespace IntervalLearningApi.Models.Topics;

public class CreateOrPatchTopic
{
    public long Id { get; set; }
    public long ParentCourseId { get; set; }
    public string Name { get; set; }
    public string Theory { get; set; }
}