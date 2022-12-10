namespace DB.Models;

public class UsersGroupEntity
{
    public long Id { get; set; }
    public string Name { get; set; }
    public long ParentCourseId { get; set; }
    public CourseEntity? ParentCourse { get; set; }

    public List<UserEntity> Users { get; set; } = new();
    public List<UserToCourseGroupEntity> UserToCourseGroupEntities { get; set; } = new();
}

public class UserToCourseGroupEntity
{
    public long ParentCourseId { get; set; }
    public CourseEntity? ParentCourse { get; set; }
    public long UserId { get; set; }
    public UserEntity? UserEntity { get; set; }
    public long UserGroupId { get; set; }
    public UsersGroupEntity? UserGroupEntity { get; set; }
}