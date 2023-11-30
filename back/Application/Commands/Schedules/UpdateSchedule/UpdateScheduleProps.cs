using Application.Commands.Schedules.CreateSchedule;

namespace Application.Commands.Schedules.UpdateSchedule;

public class UpdateScheduleProps : BaseRepeatsScheduleItem
{
    public List<UpdatePhaseInfo>? Phases { get; set; }
}