using DB.Models;
using Domain.Card.ValueObjects;
using Domain.Collection.ValueObjects;
using Domain.User.ValueObjects;

namespace Domain.Card;

//[Table("Cards")]
public class Card : AggregateRoot<ComplexCardId>
{
    // [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public CardId Id { get; }

    // [Required]
    // [StringLength(255)]
    // before FrontSideText
    public required CardText RememberingText { get; set; }

    // [StringLength(255)]
    public CardText? PromptText { get; set; }

    // [Required]
    // [StringLength(255)]
    //BEFORE BackSideText
    public required CardText MeaningText { get; set; }

    // [Required]
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

    // [StringLength(500)]
    public CardDescription? Description { get; set; }

    // [MaxLength(15)]
    // [StringLength(255)]
    public List<CardExample> Examples { get; set; } = new();
    public virtual List<RememberEntity> Remembers { get; set; } = new();
    
    public UserId ParentUserId { get; set; }
    public virtual User.User? ParentUser { get; set; }

    public CollectionId ParentCollectionId { get; set; }
    public virtual Collection.Collection? ParentCollection { get; set; }

    public Card(
        UserId parentUserId,
        CollectionId parentCollectionId,
        CardId id) 
        : base(ComplexCardId.Create(parentUserId, parentCollectionId, id))
    {
        ParentUserId = parentUserId;
        ParentCollectionId = parentCollectionId;
        Id = id;
    }

    // public static Result<Card> Create(
    //     CardId id,
    //     CardText frontSideText,
    //     CardText promptText,
    //     CardText backSideText,
    //     CardDescription description,
    //     List<CardExample> examples)
    // {
    //     return new Card(id, frontSideText, promptText, backSideText, description, examples);
    // }
    
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