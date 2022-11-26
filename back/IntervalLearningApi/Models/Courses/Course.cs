using IntervalLearningApi.Models.UsersGroups;

namespace IntervalLearningApi.Models.Courses;

public class Course
{
    public long Id { get; set; }
    public string Name { get; set; }
    public string Link { get; set; }
    public List<Topic> Topics { get; set; }
    public List<UsersGroup> UsersGroups { get; set; }
}