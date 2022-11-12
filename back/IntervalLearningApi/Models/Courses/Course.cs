namespace IntervalLearningApi.Models.Courses;

public class Course
{
    public long CourseId { get; }
    public string Name { get; }
    public List<long> UsersGroupIds { get; }

    public Course(long courseId, string name, List<long> usersGroupIds)
    {
        CourseId = courseId;
        Name = name;
        UsersGroupIds = usersGroupIds;
    }
}