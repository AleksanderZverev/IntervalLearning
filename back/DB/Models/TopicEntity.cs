using System.ComponentModel.DataAnnotations.Schema;

namespace DB.Models;

[Table("Topics")]
public class TopicEntity
{
    public long Id { get; set; }
    public string Name { get; set; }
    public string Theory { get; set; }
    public long ParentCourseId { get; set; }
    public CourseEntity? ParentCourse { get; set; }
    public List<CollectionEntity> CourseCollections { get; set; }
}