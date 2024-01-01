using Domain.Card.Events;
using Domain.Card.ValueObjects;
using Domain.Collection.ValueObjects;
using Domain.Schedule.Entities.Remember;
using Domain.Schedule.Entities.Remember.ValueObjects;
using Domain.User.ValueObjects;

namespace Domain.Card;

public class Card : AggregateRoot<ComplexCardId>
{
    public CardId Id { get; }
    public required CardText RememberingText { get; set; }
    public CardText? PromptText { get; set; }
    public required CardText MeaningText { get; set; }
    public CardDescription? Description { get; set; }
    public List<CardExample> Examples { get; set; } = new();
    public virtual List<Remember> Remembers { get; set; } = new();
    
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    
    public UserId ParentUserId { get; set; }
    public virtual User.User? ParentUser { get; set; }

    public CollectionId ParentCollectionId { get; set; }
    public virtual Collection.Collection? ParentCollection { get; set; }
    

    private List<Remember>? _orderedRemembers;
    private List<Remember> OrderedRemembers => _orderedRemembers ??= Remembers.OrderBy(r => r.RepeatedDate).ToList();

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
        => Remembers.MaxBy(c => c.RepeatedDate);
    
    public Remember? FindPreviousRememberByPhaseIndex(int phaseIndex)
    {
        //Less ID ≠ previous remember
        return OrderedRemembers
            .AsEnumerable()
            .Reverse()
            .SkipWhile(r => r.PhaseIndex != phaseIndex)
            .SkipWhile(r => r.PhaseIndex == phaseIndex)
            .FirstOrDefault();
    }

    
    public Remember? FindRememberByPhaseIndex(int phaseIndex)
    {
        return OrderedRemembers.LastOrDefault(r => r.PhaseIndex == phaseIndex);
    }

    public DateTime GetLearnedDate()
    {
        return OrderedRemembers.Last(r => r.IsAtLearnedDate()).RepeatedDate;
    }
    
    public List<Remember> GetRepeatingRemembers()
    {
        return Remembers.Where(r => !r.IsAtLearnedDate()).ToList();
    }
}