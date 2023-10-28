using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using DB.Models.Dictionary;
using Domain.Language;

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
    public Language? Language { get; set; }

    public ThemeEntity(string name)
    {
        Name = name;
    }
}