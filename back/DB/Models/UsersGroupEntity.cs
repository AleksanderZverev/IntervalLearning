namespace DB.Models;

public class UsersGroupEntity
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public long? ParentCourseId { get; set; }
    public List<UserEntity> Users { get; set; }
}