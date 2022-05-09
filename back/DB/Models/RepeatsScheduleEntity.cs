using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DB.Models;

public interface IParentRepeatsScheduleReference : IParentUserReference
{
    public short ParentRepeatsScheduleId { get; set; }
    public RepeatsScheduleEntity? ParentRepeatsSchedule { get; set; }
}

[Table("RepeatsSchedules")]
public class RepeatsScheduleEntity : IParentUserReference
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public short Id { get; set; }

    [Required]
    [StringLength(255)]
    public string Title { get; set; }
    
    public string? Description { get; set; }

    [Required] 
    public short CardsCountPerPhase { get; set; }

    [Required]
    public ForgottenBehavior ForgottenBehavior { get; set; }

    [Required]
    [MaxLength(50)]
    public virtual List<PhaseEntity> Phases { get; set; }

    public bool IsArchived { get; set; }
    public bool IsRecommended { get; set; }

    public long ParentUserId { get; set; }
    public UserEntity? ParentUser { get; set; }

    public RepeatsScheduleEntity(long parentUserId,
        short cardsCountPerPhase,
        ForgottenBehavior forgottenBehavior,
        string title,
        string? description)
    {
        ParentUserId = parentUserId;
        CardsCountPerPhase = cardsCountPerPhase;
        ForgottenBehavior = forgottenBehavior;
        Title = title;
        Description = description;
    }
}