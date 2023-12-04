using System.ComponentModel.DataAnnotations;
using Domain.Card;
using Domain.Card.ValueObjects;
using Domain.Collection.ValueObjects;
using Domain.Schedule.Entities.Remember.ValueObjects;
using Domain.Schedule.ValueObjects;
using Domain.User.ValueObjects;

namespace Domain.Schedule.Entities.Remember;

// [Table("RememberWeights")]
public class Remember : Entity<ComplexRememberId>, IParentCardReference
{
    // [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    // [Key]
    public RememberId Id { get; set; }

    /// <summary>
    /// from 0.00 to 1.00
    /// </summary>
    // [Required]
    // [Range(0d, 1d)]
    public RememberWeight Weight { get; set; }

    [Required]
    public short PhaseIndex { get; set; }

    /// <summary>
    /// Remembered or repeated date
    /// </summary>
    public DateTime RepeatedDate { get; set; }
    
    public Remember(
        UserId parentRepeatsScheduleUserId, 
        ScheduleId parentRepeatsScheduleId,
        UserId parentUserId,
        CollectionId parentCollectionId,
        CardId parentCardId,
        RememberId id,
        RememberWeight weight,
        short phaseIndex,
        DateTime repeatedDate)
        : base(new ComplexRememberId()
        {
            Id = id,
            ScheduleId = new ComplexScheduleId()
            {
                Id = parentRepeatsScheduleId,
                ParentUserId = parentRepeatsScheduleUserId
            },
            CardId = new ComplexCardId
            {
                UserId = parentUserId,
                CollectionId = parentCollectionId,
                Id = parentCardId,
            }
        })
    {
        Id = id;
        ParentUserId = parentUserId;
        ParentCollectionId = parentCollectionId;
        ParentCardId = parentCardId;
        Weight = weight;
        PhaseIndex = phaseIndex;
        RepeatedDate = repeatedDate;
        ParentRepeatsScheduleUserId = parentRepeatsScheduleUserId;
        ParentRepeatsScheduleId = parentRepeatsScheduleId;
    }

    public UserId ParentUserId { get; set; } 
    public User.User? ParentUser { get; set; }
    public CollectionId ParentCollectionId { get; set; }
    public Collection.Collection? ParentCollection { get; set; }
    public CardId ParentCardId { get; set; }
    public Card.Card? ParentCard { get; set; }

    public UserId ParentRepeatsScheduleUserId { get; set; }
    public ScheduleId ParentRepeatsScheduleId { get; set; }
    public RepeatsSchedule? ParentRepeatsSchedule { get; set; }

    public bool IsRemembered()
    {
        return Weight >= 0.70f;
    }

    public bool IsNotClearRemember()
    {
        return Weight >= 0.40f && Weight < 0.70f;
    }
}