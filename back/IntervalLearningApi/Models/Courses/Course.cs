namespace IntervalLearningApi.Models.Courses;

public class Course
{
    public long Id { get; set; }
    public string Name { get; set; }
    public string Description { get; }
    public string Link { get; set; }
    public HashSet<long> AdminIds { get; set; }
}