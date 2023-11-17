using System.ComponentModel.DataAnnotations.Schema;
using DB.Models.ValueObjects;
using Domain.Schedule;
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
        ScheduleId parentRepeatsScheduleId,
        PhaseId parentPhaseId,
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

    public ScheduleId ParentRepeatsScheduleId { get; set; }
    public RepeatsSchedule? ParentRepeatsSchedule { get; set; }

    public PhaseId ParentPhaseId { get; set; }
    public Phase ParentPhase { get; set; }
}