using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DB.Models;

public interface IParentCollectionReference : IParentUserReference
{
    public short ParentCollectionId { get; set; }
    public CollectionEntity? ParentCollection { get; set; }
}

public interface ICreateOrEditModel
{
    public string Title { get; }
    public bool IsDefaultBackSide { get; }
    public short ThemeId { get; }
    public long ParentUserId { get; }
}

[Table("Collections")]
public class CollectionEntity : IParentUserReference, ICreateOrEditModel
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public short Id { get; set; }
    
    [Required]
    [StringLength(100)]
    public string Title { get; set; }

    public bool IsDefaultBackSide { get; set; }

    public short ThemeId { get; set; }
    public virtual ThemeEntity? Theme { get; set; }

    [Required]
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    public short CardsCount { get; set; }
    [NotMapped]
    public short NotStartedCardsCount { get; set; }
    //public short StartedCards { get; set; }
    //public short FinishedCards { get; set; }

    public virtual List<CardEntity> Cards { get; set; } = new();

    public long ParentUserId { get; set; }
    public virtual UserEntity? ParentUser { get; set; }
}