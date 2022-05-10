using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DB.Models;

public interface IParentRepeatsScheduleReference : IParentUserReference
{
    public short ParentRepeatsScheduleId { get; set; }
    public RepeatsScheduleEntity? ParentRepeatsSchedule { get; set; }
}

public interface IPatchRepeatsSchedule
{
    public string Title { get; }
    public string? ShortDescription { get; }
    public string? Description { get; }
    public short CardsCountPerPhase { get; }
    public ForgottenBehavior ForgottenBehavior { get; }
    public string? DefaultPhaseShortDescription { get; }
    public string? DefaultPhaseDescription { get; }
    public string? DefaultRepeatPhaseShortDescription { get; }
    public string? DefaultRepeatPhaseDescription { get; }
}

public interface ICreateOrPatchRepeatsSchedule : IPatchRepeatsSchedule
{
    public long ParentUserId { get; }
}

public class PatchRepeatsSchedule : IPatchRepeatsSchedule
{
    public string Title { get; }
    public string? ShortDescription { get;  }
    public string? Description { get;  }
    public short CardsCountPerPhase { get;  }
    public ForgottenBehavior ForgottenBehavior { get;  }
    public string? DefaultPhaseShortDescription { get;  }
    public string? DefaultPhaseDescription { get;  }
    public string? DefaultRepeatPhaseShortDescription { get;  }
    public string? DefaultRepeatPhaseDescription { get;  }

    public PatchRepeatsSchedule(
        short cardsCountPerPhase,
        ForgottenBehavior forgottenBehavior,
        string title,
        string? shortDescription,
        string? description,
        string? defaultPhaseShortDescription,
        string? defaultPhaseDescription,
        string? defaultRepeatPhaseShortDescription,
        string? defaultRepeatPhaseDescription)
    {
        CardsCountPerPhase = cardsCountPerPhase;
        ForgottenBehavior = forgottenBehavior;
        Title = title;
        Description = description;
        ShortDescription = shortDescription;
        DefaultPhaseShortDescription = defaultPhaseShortDescription;
        DefaultPhaseDescription = defaultPhaseDescription;
        DefaultRepeatPhaseShortDescription = defaultRepeatPhaseShortDescription;
        DefaultRepeatPhaseDescription = defaultRepeatPhaseDescription;
    }
}

public class CreateScheduleItem : PatchRepeatsSchedule, ICreateOrPatchRepeatsSchedule
{
    public long ParentUserId { get; set; }

    public CreateScheduleItem(
        long parentUserId,
        short cardsCountPerPhase,
        ForgottenBehavior forgottenBehavior,
        string title,
        string? shortDescription,
        string? description,
        string? defaultPhaseShortDescription,
        string? defaultPhaseDescription,
        string? defaultRepeatPhaseShortDescription,
        string? defaultRepeatPhaseDescription)
        :
        base(
            cardsCountPerPhase,
            forgottenBehavior,
            title,
            shortDescription,
            description,
            defaultPhaseShortDescription,
            defaultPhaseDescription,
            defaultRepeatPhaseShortDescription,
            defaultRepeatPhaseDescription)

    {
        ParentUserId = parentUserId;
    }
}

[Table("RepeatsSchedules")]
public class RepeatsScheduleEntity : IParentUserReference, ICreateOrPatchRepeatsSchedule
{
    private const int ShortDescriptionLength = 100;

    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public short Id { get; set; }

    [Required]
    [StringLength(255)]
    public string Title { get; set; }

    [StringLength(ShortDescriptionLength)]
    public string? ShortDescription { get; set; }

    [Column("OnStartLearningDescription")]
    public string? Description { get; set; }

    [StringLength(ShortDescriptionLength)]
    public string? DefaultPhaseShortDescription { get; set; }
    public string? DefaultPhaseDescription { get; set; }

    [StringLength(ShortDescriptionLength)]
    public string? DefaultRepeatPhaseShortDescription { get; set; }
    public string? DefaultRepeatPhaseDescription { get; set; }

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
}