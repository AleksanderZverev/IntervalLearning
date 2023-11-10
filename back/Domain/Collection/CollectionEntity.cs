using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using DB.Models.Store;
using Domain.Collection;
using Domain.Collection.ValueObjects;
using Domain.User;
using Domain.User.ValueObjects;

namespace DB.Models;

public interface IParentCollectionReference : IParentUserReference
{
    public CollectionId ParentCollectionId { get; set; }
    public Collection? ParentCollection { get; set; }
}

public interface ICreateOrEditModel
{
    public string Title { get; }
    public bool IsDefaultBackSide { get; }
    public short ThemeId { get; }
    public UserId ParentUserId { get; }
}

// [Table("Collections")]
// public class CollectionEntity : IParentUserReference, ICreateOrEditModel
// {
//     [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
//     public short Id { get; set; }
//     
//     [Required]
//     [StringLength(100)]
//     public string Title { get; set; }
//
//     public bool IsDefaultBackSide { get; set; }
//
//     public short ThemeId { get; set; }
//     public virtual ThemeEntity? Theme { get; set; }
//
//     [Required]
//     public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
//
//     public short CardsCount { get; set; }
//     [NotMapped]
//     public short NotStartedCardsCount { get; set; }
//     //public short StartedCards { get; set; }
//     //public short FinishedCards { get; set; }
//
//     public virtual List<CardEntity> Cards { get; set; } = new();
//
//     public UserId ParentUserId { get; set; }
//     public virtual User? ParentUser { get; set; }
//
//     public bool IsPublic { get; set; }
//
//     public CollectionPublicationEntity? CollectionPublicationEntity { get; set; }
// }