using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Domain.Language;
using Domain.Language.ValueObjects;

namespace DB.Models.Dictionary;

[Table("Words")]
public class WordEntity
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [Required]
    [StringLength(255)]
    public string Word { get; set; }

    [StringLength(255)]
    public string? Pronunciation { get; set; }


    public LanguageId LanguageId { get; set; }
    public Language? Language { get; set; }
}