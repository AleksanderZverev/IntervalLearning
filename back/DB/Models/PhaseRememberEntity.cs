using System.ComponentModel.DataAnnotations.Schema;

namespace DB.Models;

public class PhaseRememberEntity : IParentPhaseReference
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public short Id { get; set; }
    public long RepeatedUserId { get; set; }
    public UserEntity? RepeatedUser { get; set; }

    public float Weight { get; set; }

    public PhaseRememberEntity(
        long parentUserId,
        short parentRepeatsScheduleId,
        short parentPhaseId,
        long repeatedUserId,
        float weight)
    {
        ParentUserId = parentUserId;
        ParentRepeatsScheduleId = parentRepeatsScheduleId;
        ParentPhaseId = parentPhaseId;
        RepeatedUserId = repeatedUserId;
        Weight = weight;
    }

    public long ParentUserId { get; set; }
    public UserEntity? ParentUser { get; set; }

    public short ParentRepeatsScheduleId { get; set; }
    public RepeatsScheduleEntity? ParentRepeatsSchedule { get; set; }

    public short ParentPhaseId { get; set; }
    public PhaseEntity ParentPhase { get; set; }
}