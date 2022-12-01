using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DB.Models;

[Table("Courses")]
public class CourseEntity
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public long Id { get; set; }
    [Required]
    [StringLength(255)]
    public string Name { get; set; }
    public string Description { get; set; }
    public string Link { get; }
    public bool IsPrivate { get; set; }
    public HashSet<long> AdminIds { get; } = new();
    public List<TopicEntity> Topics { get; } = new();
    public List<UsersGroupEntity> UsersGroups { get; } = new();
}