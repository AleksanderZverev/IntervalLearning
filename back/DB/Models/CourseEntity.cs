using System.ComponentModel.DataAnnotations.Schema;

namespace DB.Models;

[Table("Courses")]
public class CourseEntity
{
    public long Id { get; set; }
    public string Name { get; set; }
    public List<TopicEntity> Topics { get; set; }
    public List<long> UsersGroupIds { get; set; }
}