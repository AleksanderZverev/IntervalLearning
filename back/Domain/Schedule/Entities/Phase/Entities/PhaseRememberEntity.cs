using Domain.Schedule.Entities.Phase.ValueObjects;
using Domain.Schedule.ValueObjects;
using Domain.User.ValueObjects;

namespace Domain.Schedule.Entities.Phase.Entities;

public class PhaseRememberEntity : IParentPhaseReference
{
    public int Id { get; set; }
    public UserId RepeatedUserId { get; set; }
    public User.User? RepeatedUser { get; set; }

    public float Weight { get; set; }

    public PhaseRememberEntity(
        int id,
        UserId parentUserId,
        ScheduleId parentRepeatsScheduleId,
        PhaseId parentPhaseId,
        UserId repeatedUserId,
        float weight)
    {
        Id = id;
        ParentUserId = parentUserId;
        ParentRepeatsScheduleId = parentRepeatsScheduleId;
        ParentPhaseId = parentPhaseId;
        RepeatedUserId = repeatedUserId;
        Weight = weight;
    }

    public UserId ParentUserId { get; set; }
    public User.User? ParentUser { get; set; }

    public ScheduleId ParentRepeatsScheduleId { get; set; }
    public RepeatsSchedule? ParentRepeatsSchedule { get; set; }

    public PhaseId ParentPhaseId { get; set; }
    public Phase ParentPhase { get; set; }
}