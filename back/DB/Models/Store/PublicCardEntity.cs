using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DB.Models.Store;

[Table("PublicCards")]
public class PublicCardEntity
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public short Id { get; set; }

    [Required]
    [StringLength(255)]
    public string RememberingText { get; set; } = string.Empty;

    [StringLength(255)]
    public string PromptText { get; set; } = string.Empty;

    [StringLength(255)]
    public string MeaningText { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Description { get; set; }

    [MaxLength(15)]
    [StringLength(255)]
    public List<string>? Examples { get; set; }

    public long OwnerUserId { get; set; }
    public UserEntity? OwnerUser { get; set; }

    public short PublicCollectionId { get; set; }
    public PublicCollectionEntity? PublicCollection { get; set; }
}