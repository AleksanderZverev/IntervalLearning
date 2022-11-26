using IntervalLearningApi.Models.ByUser;

namespace IntervalLearningApi.Models.UsersGroups;

public class UsersGroup
{
    public long Id { get; set; }
    public string Name { get; set; }
    public long ParentCourseId { get; set; }
    public List<UserInfo> Users { get; set; }
}