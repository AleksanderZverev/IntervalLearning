using DB.Models.ValueObjects;
using Domain.User.ValueObjects;
using IntervalLearningApi.Constants;
using IntervalLearningApi.Extensions;
using IntervalLearningApi.Models.ByUser;
using IntervalLearningApi.Models.RepeatsSchedule;
using IntervalLearningApi.Services;
using MapsterMapper;
using Microsoft.AspNetCore.Mvc;

namespace IntervalLearningApi.Controllers
{
    [Route(ApiRoutes.Schedule.BasePath)]
    [Authorize]
    [ApiController]
    public class RepeatsScheduleController : ControllerBase
    {
        private readonly IMapper mapper;
        private readonly RepeatsScheduleService repeatsScheduleService;

        public RepeatsScheduleController(
            IMapper mapper,
            RepeatsScheduleService repeatsScheduleService)
        {
            this.mapper = mapper;
            this.repeatsScheduleService = repeatsScheduleService;
        }

        [HttpGet(ApiRoutes.Schedule.Get_GetAll)]
        public ActionResult<List<RepeatsScheduleDto>> GetAll()
        {
            var userId = HttpContext.GetUserId();

            if (userId.IsFailed)
                return BadRequest();

            var schedules = repeatsScheduleService.GetAll(userId.Value);
            return mapper.Map<List<RepeatsScheduleDto>>(schedules);
        }

        [HttpGet(ApiRoutes.Schedule.Get_GetUserSchedule)]
        public ActionResult<RepeatsScheduleDto> GetSchedule(long userId, short scheduleId)
        {
            var schedule = repeatsScheduleService.Find(UserId.Create(userId).Value, ScheduleId.Create(scheduleId).Value);
            return schedule == null ? NotFound() : mapper.Map<RepeatsScheduleDto>(schedule);
        }

        [HttpGet(ApiRoutes.Schedule.Get_GetMySchedule)]
        public ActionResult<RepeatsScheduleDto> GetSchedule(short scheduleId)
        {
            var userId = HttpContext.GetUserId();

            if (userId.IsFailed)
                return BadRequest();

            var schedule = repeatsScheduleService.Find(userId.Value, ScheduleId.Create(scheduleId).Value);
            return schedule == null ? NotFound() :  mapper.Map<RepeatsScheduleDto>(schedule);
        }

        [HttpPatch(ApiRoutes.Schedule.Patch_EditSchedule)]
        public async Task<ActionResult<RepeatsScheduleDto>> EditSchedule(short scheduleId, [FromBody] UpdateScheduleRequest request)
        {
            var userId = HttpContext.GetUserId();

            if (userId.IsFailed)
                return BadRequest();

            var (schedule, error) = await repeatsScheduleService.PatchSchedule(
                userId.Value, 
                ScheduleId.Create(scheduleId).Value, 
                mapper.Map<UpdateScheduleItem>(request));
            
            return schedule != null 
                ?  mapper.Map<RepeatsScheduleDto>(schedule) 
                : BadRequest(error);
        }

        [HttpPost(ApiRoutes.Schedule.Post_CreateSchedule)]
        public async Task<ActionResult<RepeatsScheduleDto>> CreateSchedule([FromBody] CreateScheduleRequest request)
        {
            var userId = HttpContext.GetUserId();

            if (userId.IsFailed)
                return BadRequest();

            var (schedule, error) = await repeatsScheduleService.Create(
                userId.Value, 
                mapper.Map<CreateScheduleItem>(request));
            
            return schedule != null 
                ? mapper.Map<RepeatsScheduleDto>(schedule) 
                : BadRequest(error);
        }
    }
}
