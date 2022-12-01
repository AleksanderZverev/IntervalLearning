using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DB.Models;

[Table("Topics")]
public class TopicEntity
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long Id { get; set; }
    [Required]
    [StringLength(255)]
    public string Name { get; set; }
    public string Theory { get; set; }
    public long ParentCourseId { get; set; }
    public CourseEntity? ParentCourse { get; set; }
    public List<TopicCollectionEntity> TopicsCollections { get; set; }
}