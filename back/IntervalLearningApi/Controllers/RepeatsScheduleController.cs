using System.ComponentModel.DataAnnotations;
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

        [HttpPost]
        public async Task<ActionResult<Schedule>> CreateSchedule([FromBody] CreateScheduleRequest request)
        {
            var userId = HttpContext.GetUserId();
            var (schedule, error) = await repeatsScheduleService.Create(userId, request);
            return schedule != null ? ToSchedule(schedule) : BadRequest(error);
        }

        public class CreateScheduleRequest
        {
            [Required]
            public short CardsCountPerPhase { get; set; }
            [Required]
            public int ForgottenBehavior { get; set; }
            [Required]
            public string Title { get; set; }

            [StringLength(100)]
            public string? ShortDescription { get; }

            [StringLength(1000)]
            public string? Description { get; set; }
            [Required]
            public List<PhaseInfo> Phases { get; set; }

            [StringLength(100)]
            public string? DefaultPhaseShortDescription { get; }
            [StringLength(1000)]
            public string? DefaultPhaseDescription { get; }
            [StringLength(100)]
            public string? DefaultRepeatPhaseShortDescription { get; }
            [StringLength(1000)]
            public string? DefaultRepeatPhaseDescription { get; }
        }

        private static Schedule ToSchedule(RepeatsScheduleEntity schedule)
        {
            return new Schedule(
                schedule.ParentUserId,
                schedule.Id,
                schedule.Title,
                schedule.CardsCountPerPhase,
                schedule.ShortDescription,
                schedule.Description,
                schedule.ForgottenBehavior,
                schedule.IsRecommended,
                schedule.Phases.Select(ToPhase).ToList(),
                schedule.DefaultPhaseShortDescription,
                schedule.DefaultPhaseDescription,
                schedule.DefaultRepeatPhaseShortDescription,
                schedule.DefaultRepeatPhaseDescription);
        }

        public static Phase ToPhase(PhaseEntity phase)
        {
            return new Phase(
                phase.ParentUserId,
                phase.ParentRepeatsScheduleId,
                phase.Id,
                phase.SecondsFromLastPhase,
                phase.ShortDescription,
                phase.Description);
        }
    }
}
