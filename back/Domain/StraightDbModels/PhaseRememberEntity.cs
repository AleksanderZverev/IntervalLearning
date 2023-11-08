using System.ComponentModel.DataAnnotations.Schema;
using Domain.User;
using Domain.User.ValueObjects;

namespace DB.Models;

public class PhaseRememberEntity : IParentPhaseReference
{
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public short Id { get; set; }
    public UserId RepeatedUserId { get; set; }
    public User? RepeatedUser { get; set; }

    public float Weight { get; set; }

    public PhaseRememberEntity(
        UserId parentUserId,
        short parentRepeatsScheduleId,
        short parentPhaseId,
        UserId repeatedUserId,
        float weight)
    {
        ParentUserId = parentUserId;
        ParentRepeatsScheduleId = parentRepeatsScheduleId;
        ParentPhaseId = parentPhaseId;
        RepeatedUserId = repeatedUserId;
        Weight = weight;
    }

    public UserId ParentUserId { get; set; }
    public User? ParentUser { get; set; }

    public short ParentRepeatsScheduleId { get; set; }
    public RepeatsScheduleEntity? ParentRepeatsSchedule { get; set; }

    public short ParentPhaseId { get; set; }
    public PhaseEntity ParentPhase { get; set; }
}