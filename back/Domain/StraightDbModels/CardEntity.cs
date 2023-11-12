using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Domain.Collection;
using Domain.Collection.ValueObjects;
using Domain.User;
using Domain.User.ValueObjects;

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

    public UserId ParentUserId { get; }
    public CollectionId ParentCollectionId { get; }
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

    public UserId ParentUserId { get; set; }
    public virtual User? ParentUser { get; set; }

    public CollectionId ParentCollectionId { get; set; }
    public virtual Collection? ParentCollection { get; set; }

    public RememberEntity? FindLastRemember() 
        => Remembers.MaxBy(c => c.Id);
    
    public DateTime GetLearnedDate()
    {
        return Remembers
            .OrderBy(r => r.RepeatedDate)
            .First()
            .RepeatedDate;
    }
    
    public List<RememberEntity> GetRepeatingRemembers()
    {
        var learnedDate = GetLearnedDate();
        return Remembers
            .Where(r => r.RepeatedDate.Date != learnedDate.Date)
            .ToList();
    }
}