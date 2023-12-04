using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Domain.Common.ValueObjects;
using Domain.Language.ValueObjects;
using Domain.Theme.ValueObjects;

namespace Domain.Theme;

// [Table("Themes")]
public class Theme : Entity<ThemeId>
{
    // [Required]
    // [StringLength(100)]
    public required ThemeTitle Name { get; init; }

    public LanguageId? LanguageId { get; set; }
    public virtual Language.Language? Language { get; set; }

    public Theme(ThemeId id) : base(id)
    {
    }
}