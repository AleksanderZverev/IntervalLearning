using System.ComponentModel.DataAnnotations.Schema;

namespace DB.Models;

[Table("Courses")]
public class CourseEntity
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long Id { get; }
    public string Name { get; }
    public string Description { get; }
    public string Link { get; }
    public List<TopicEntity> Topics { get; } = new();
    public List<UsersGroupEntity> UsersGroups { get; } = new();
}