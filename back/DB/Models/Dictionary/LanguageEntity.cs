using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DB.Models.Dictionary;

[Table("Languages")]
public class LanguageEntity
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public short Id { get; set; }

    [Required]
    [StringLength(50)]
    public string Name { get; set; }

    [Required]
    [StringLength(50)]
    public string NativeLanguageName { get; set; }

    [StringLength(50)]
    public string? TranslationLinkTitle { get; set; }
    public string? TranslationLink { get; set; }
}