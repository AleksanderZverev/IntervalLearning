using System.ComponentModel.DataAnnotations.Schema;

namespace DB.Models;

[Table("Topics")]
public class TopicEntity
{
    public Guid Id { get; set; }
    public Guid ParentCourseId { get; set; }
    public string Name { get; set; }
    public List<CardEntity> CourseCollections { get; set; }
    public string Text { get; set; }
}