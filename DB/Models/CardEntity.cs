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
    public string FrontSideText { get; set; }
    public string BackSideText { get; set; }
    public string? Description { get; set; }
    public List<string>? Examples { get; set; }

    public long ParentUserId { get; set; }
    public short ParentCollectionId { get; set; }
}


[Table("Cards")]
public class CardEntity : ICreateOrPatchCard, IParentCollectionReference
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public short Id { get; set; }

    [Required]
    [StringLength(255)]
    public string FrontSideText { get; set; }

    [Required]
    [StringLength(255)]
    public string BackSideText { get; set; }

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