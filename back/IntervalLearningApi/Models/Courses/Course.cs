namespace IntervalLearningApi.Models.Courses;

public class Course
{
    public long CourseId { get; set; }
    public string Name { get; set; }
    public string Link { get; set; }
    public List<UsersGroup> UsersGroups { get; set; }
}