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
        public ActionResult<Schedule> CreateSchedule([FromBody] CreateScheduleItem item)
        {
            var userId = HttpContext.GetUserId();
            var (schedule, error) = repeatsScheduleService.Create(
                userId,
                item.CardsCountPerPhase,
                (ForgottenBehavior)item.ForgottenBehavior,
                item.Title,
                item.Phases,
                item.Description);

            return schedule != null ? Ok(ToSchedule(schedule)) : BadRequest(error);
        }

        public class CreateScheduleItem
        {
            [Required]
            public short CardsCountPerPhase { get; set; }
            [Required]
            public int ForgottenBehavior { get; set; }
            [Required]
            public string Title { get; set; }
            [StringLength(500)]
            public string? Description { get; set; }
            [Required]
            public List<PhaseInfo> Phases { get; set; }
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
                schedule.IsRecommended,
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
