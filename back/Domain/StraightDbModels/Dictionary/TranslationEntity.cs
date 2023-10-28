using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Domain.Language;

namespace DB.Models.Dictionary;

[Table("Translations")]
public class TranslationEntity
{
    public int WordId { get; set; }
    public WordEntity? Word { get; set; }

    public short LanguageId { get; set; }
    public Language? Language { get; set; }

    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public short Id { get; set; }

    [Required]
    [StringLength(255)]
    public string Translation { get; set; }
}