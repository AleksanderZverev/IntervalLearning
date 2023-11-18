using DB.Models.ValueObjects;
using Domain;
using Domain.Card;
using Domain.Card.ValueObjects;
using Domain.Collection;
using Domain.Collection.ValueObjects;
using Domain.Schedule;
using Domain.User;
using Domain.User.ValueObjects;

namespace DB.Models;

// [Table("Queue")]
public class CardRepeatQueue : AggregateRoot<ComplexQueueId>, IParentCardReference
{
    // [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public QueueId Id { get; set; }
    public short PhaseIndex { get; set; }

    public DateTime Date { get; set; }

    public CardRepeatQueue(
        UserId parentRepeatsScheduleUserId,
        ScheduleId parentRepeatsScheduleId,
        UserId parentUserId, 
        CollectionId parentCollectionId, 
        CardId parentCardId,
        QueueId id,
        short phaseIndex, 
        DateTime date) : base(new ComplexQueueId()
    {
        QueueId = id,
        CardId = ComplexCardId.Create(parentUserId, parentCollectionId, parentCardId),
        ScheduleId = new ComplexScheduleId()
        {
            Id = parentRepeatsScheduleId,
            ParentUserId = parentRepeatsScheduleUserId,
        }
    })
    {
        Id = id;
        ParentUserId = parentUserId;
        ParentCollectionId = parentCollectionId;
        ParentCardId = parentCardId;
        PhaseIndex = phaseIndex;
        Date = date;
        ParentRepeatsScheduleUserId = parentRepeatsScheduleUserId;
        ParentRepeatsScheduleId = parentRepeatsScheduleId;
    }

    public UserId ParentUserId { get; set; }

    public User? ParentUser { get; set; }

    public CollectionId ParentCollectionId { get; set; }
    public Collection? ParentCollection { get; set; }

    public CardId ParentCardId { get; set; }
    public Card? ParentCard { get; set; }

    public UserId ParentRepeatsScheduleUserId { get; set; }
    public ScheduleId ParentRepeatsScheduleId { get; set; }
    public RepeatsSchedule? ParentRepeatsSchedule { get; set; }
}