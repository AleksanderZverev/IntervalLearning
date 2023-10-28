using System.ComponentModel.DataAnnotations;
using DB.Models;
using IntervalLearningApi.Constants;
using IntervalLearningApi.Extensions;
using IntervalLearningApi.Models.RepeatsSchedule;
using IntervalLearningApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace IntervalLearningApi.Controllers
{
    [Route(ApiRoutes.Schedule.BasePath)]
    [Authorize]
    [ApiController]
    public class RepeatsScheduleController : ControllerBase
    {
        private readonly RepeatsScheduleService repeatsScheduleService;

        public RepeatsScheduleController(RepeatsScheduleService repeatsScheduleService)
        {
            this.repeatsScheduleService = repeatsScheduleService;
        }

        [HttpGet(ApiRoutes.Schedule.Get_GetAll)]
        public List<Schedule> GetAll()
        {
            var userId = HttpContext.GetUserId();
            return repeatsScheduleService.GetAll(userId).Select(ToSchedule).ToList();
        }

        [HttpGet(ApiRoutes.Schedule.Get_GetUserSchedule)]
        public ActionResult<Schedule> GetSchedule(long userId, short scheduleId)
        {
            var schedule = repeatsScheduleService.Find(userId, scheduleId);
            return schedule == null ? NotFound() : ToSchedule(schedule);
        }

        [HttpGet(ApiRoutes.Schedule.Get_GetMySchedule)]
        public ActionResult<Schedule> GetSchedule(short scheduleId)
        {
            var userId = HttpContext.GetUserId();
            var schedule = repeatsScheduleService.Find(userId, scheduleId);
            return schedule == null ? NotFound() : ToSchedule(schedule);
        }

        [HttpPatch(ApiRoutes.Schedule.Patch_EditSchedule)]
        public async Task<ActionResult<Schedule>> EditSchedule(short scheduleId, [FromBody] UpdateScheduleRequest request)
        {
            var userId = HttpContext.GetUserId();
            var (schedule, error) = await repeatsScheduleService.PatchSchedule(userId, scheduleId, request);
            return schedule != null ? ToSchedule(schedule) : BadRequest(error);
        }

        [HttpPost(ApiRoutes.Schedule.Post_CreateSchedule)]
        public async Task<ActionResult<Schedule>> CreateSchedule([FromBody] CreateScheduleRequest request)
        {
            var userId = HttpContext.GetUserId();
            var (schedule, error) = await repeatsScheduleService.Create(userId, request);
            return schedule != null ? ToSchedule(schedule) : BadRequest(error);
        }

        public class UpdateScheduleRequest
        {
            [Required]
            public short CardsCountPerPhase { get; set; }
            [Required]
            public string Title { get; set; }

            [StringLength(200)]
            public string? ShortDescription { get; set; }

            [StringLength(1000)]
            public string? Description { get; set; }

            public List<UpdatePhaseInfo>? Phases { get; set; }

            [StringLength(200)]
            public string? DefaultPhaseShortDescription { get; set; }
            [StringLength(1000)]
            public string? DefaultPhaseDescription { get; set; }
            [StringLength(200)]
            public string? DefaultRepeatPhaseShortDescription { get; set; }
            [StringLength(1000)]
            public string? DefaultRepeatPhaseDescription { get; set; }
        }

        public class CreateScheduleRequest
        {
            [Required]
            public short CardsCountPerPhase { get; set; }
            [Required]
            public int ForgottenBehavior { get; set; }
            [Required]
            public string Title { get; set; }

            [StringLength(200)]
            public string? ShortDescription { get; set; }

            [StringLength(1000)]
            public string? Description { get; set; }
            [Required]
            public List<PhaseInfo> Phases { get; set; }

            [StringLength(200)]
            public string? DefaultPhaseShortDescription { get; set; }
            [StringLength(1000)]
            public string? DefaultPhaseDescription { get; set; }
            [StringLength(200)]
            public string? DefaultRepeatPhaseShortDescription { get; set; }
            [StringLength(1000)]
            public string? DefaultRepeatPhaseDescription { get; set; }
        }

        private static Schedule ToSchedule(RepeatsScheduleEntity schedule)
        {
            return new Schedule(
                schedule.ParentUserId,
                schedule.Id,
                schedule.Title,
                schedule.CardsCountPerPhase,
                schedule.ShortDescription,
                schedule.OnStartLearningDescription,
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
                phase.OnLearnDescription,
                phase.IsDefaultValueSide);
        }
    }
}
