namespace IntervalLearningApi.Models.Courses;

public class Course
{
    public long CourseId { get; }
    public string Name { get; }
    public string Link { get; set; }
    public List<long> UsersGroupIds { get; }

    public Course(long courseId, string name, string link, List<long> usersGroupIds)
    {
        CourseId = courseId;
        Name = name;
        Link = link;
        UsersGroupIds = usersGroupIds;
    }
}