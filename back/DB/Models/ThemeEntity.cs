using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using DB.Models.Dictionary;

namespace DB.Models;

[Table("Themes")]
public class ThemeEntity
{
    [Key]
    public short Id { get; set; }

    [Required]
    [StringLength(100)]
    public string Name { get; set; }

    public short? LanguageId { get; set; }
    public LanguageEntity? Language { get; set; }

    public ThemeEntity(string name)
    {
        Name = name;
    }
}