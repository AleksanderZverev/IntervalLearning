using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DB.Models;

[Table("Themes")]
public class ThemeEntity
{
    [Key]
    public short Id { get; set; }

    [Required]
    [StringLength(100)]
    public string Name { get; set; }

    public ThemeEntity(string name)
    {
        Name = name;
    }
}