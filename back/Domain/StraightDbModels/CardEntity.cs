using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DB.Models;

public interface IParentCardReference : IParentCollectionReference
{
    public short ParentCardId { get; set; }
    public CardEntity? ParentCard { get; set; }
}

public interface ICreateOrPatchCard
{
    public string RememberingText { get; }
    public string PromptText { get; }
    public string MeaningText { get; }
    public string? Description { get; }
    public List<string>? Examples { get; }

    public long ParentUserId { get; }
    public short ParentCollectionId { get; }
}


[Table("Cards")]
public class CardEntity : ICreateOrPatchCard, IParentCollectionReference
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public short Id { get; set; }

    [Required]
    [StringLength(255)]
    public string RememberingText { get; set; }

    [StringLength(255)]
    public string PromptText { get; set; }

    [Required]
    [StringLength(255)]
    public string MeaningText { get; set; }

    [Required]
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    [StringLength(500)]
    public string? Description { get; set; }

    [MaxLength(15)]
    [StringLength(255)]
    public List<string>? Examples { get; set; }

    public virtual List<RememberEntity> Remembers { get; set; } = new();

    public long ParentUserId { get; set; }
    public UserEntity? ParentUser { get; set; }

    public short ParentCollectionId { get; set; }
    public CollectionEntity? ParentCollection { get; set; }
}