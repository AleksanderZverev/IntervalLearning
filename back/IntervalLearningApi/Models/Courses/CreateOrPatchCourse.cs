namespace IntervalLearningApi.Models.Courses;

public class CreateOrPatchCourse
{
    public string Name;
    public List<long> UsersGroupIds;
}