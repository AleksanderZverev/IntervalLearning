using System.ComponentModel.DataAnnotations.Schema;

namespace DB.Models;

[Table("Courses")]
public class CourseEntity
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long Id { get; }
    public string Name { get; set; }
    public string Description { get; set; }
    public string Link { get; }
    public HashSet<long> AdminIds { get; } = new();
    public List<TopicEntity> Topics { get; } = new();
    public List<UsersGroupEntity> UsersGroups { get; } = new();
}