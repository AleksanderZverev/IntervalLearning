using DB.Models;
using DB.Models.ValueObjects;
using Domain.Card.Events;
using Domain.Card.ValueObjects;
using Domain.Collection.ValueObjects;
using Domain.Schedule.Entities.Remember;
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
    public virtual List<Remember> Remembers { get; set; } = new();
    
    public UserId ParentUserId { get; set; }
    public virtual User.User? ParentUser { get; set; }

    public CollectionId ParentCollectionId { get; set; }
    public virtual Collection.Collection? ParentCollection { get; set; }

    protected Card() : base()
    {
        //For EF
    }

    public Card(
        UserId parentUserId,
        CollectionId parentCollectionId,
        CardId id) 
        : base(new ComplexCardId
        {
            UserId = parentUserId,
            CollectionId = parentCollectionId,
            Id = id,
        })
    {
        ParentUserId = parentUserId;
        ParentCollectionId = parentCollectionId;
        Id = id;
        AddDomainEvent(new CardCreatedEvent(this));
    }

    public void Delete()
    {
        AddDomainEvent(new CardDeletedEvent(this));
    }

    public Remember? FindLastRemember() 
        => Remembers.MaxBy(c => c.Id);
    
    public Remember? FindPreviousRemember(RememberId rememberId) 
        => Remembers.FindLast(r => r.Id < rememberId);
    
    public DateTime GetLearnedDate()
    {
        return Remembers
            .OrderBy(r => r.RepeatedDate)
            .First()
            .RepeatedDate;
    }
    
    public List<Remember> GetRepeatingRemembers()
    {
        var learnedDate = GetLearnedDate();
        return Remembers
            .Where(r => r.RepeatedDate.Date != learnedDate.Date)
            .ToList();
    }
}