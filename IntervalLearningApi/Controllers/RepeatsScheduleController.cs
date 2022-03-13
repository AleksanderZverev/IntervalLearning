using DB.Models;
using IntervalLearningApi.Extensions;
using IntervalLearningApi.Models.RepeatsSchedule;
using IntervalLearningApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace IntervalLearningApi.Controllers
{
    [Route("api/schedules")]
    [Authorize]
    [ApiController]
    public class RepeatsScheduleController : ControllerBase
    {
        private readonly RepeatsScheduleService repeatsScheduleService;

        public RepeatsScheduleController(RepeatsScheduleService repeatsScheduleService)
        {
            this.repeatsScheduleService = repeatsScheduleService;
        }

        [HttpGet]
        public List<Schedule> GetAll()
        {
            var userId = HttpContext.GetUserId();
            return repeatsScheduleService.GetAll(userId).Select(ToSchedule).ToList();
        }

        private static Schedule ToSchedule(RepeatsScheduleEntity schedule)
        {
            return new Schedule(
                schedule.ParentUserId.ToString(),
                schedule.Id,
                schedule.Title,
                schedule.CardsCountPerPhase,
                schedule.Description,
                schedule.ForgottenBehavior,
                schedule.Phases.Select(ToPhase).ToList());
        }

        private static Phase ToPhase(PhaseEntity phase)
        {
            return new Phase(
                phase.ParentUserId.ToString(),
                phase.ParentRepeatsScheduleId,
                phase.Id,
                phase.SecondsFromLastPhase,
                phase.Description);
        }
    }
}
