using Application.Commands.Schedules.CreateSchedule;
using Application.Commands.Schedules.GetAvailableSchedules;
using Application.Commands.Schedules.GetSchedule;
using DB.Models.ValueObjects;
using Domain.User.ValueObjects;
using IntervalLearningApi.Constants;
using IntervalLearningApi.Extensions;
using IntervalLearningApi.Infrastructure.CommandManager;
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
        private readonly CommandManager commandManager;
        private readonly RepeatsScheduleService repeatsScheduleService;

        public RepeatsScheduleController(
            IMapper mapper,
            CommandManager commandManager,
            RepeatsScheduleService repeatsScheduleService)
        {
            this.mapper = mapper;
            this.commandManager = commandManager;
            this.repeatsScheduleService = repeatsScheduleService;
        }

        [HttpGet(ApiRoutes.Schedule.Get_GetAll)]
        public async Task<ActionResult<List<RepeatsScheduleDto>>> GetAll()
        {
            var userId = HttpContext.GetUserId();

            if (userId.IsFailed)
                return BadRequest();

            var schedulesResult = await commandManager
                .GetCommand<GetAvailableSchedulesCommand>()
                .Handle(new GetAvailableSchedulesRequest(userId.Value));

            return schedulesResult.ToActionResult(schedules => mapper.Map<List<RepeatsScheduleDto>>(schedules));
        }

        [HttpGet(ApiRoutes.Schedule.Get_GetUserSchedule)]
        public async Task<ActionResult<RepeatsScheduleDto>> GetSchedule(long userId, short scheduleId)
        {
            var scheduleResult = await commandManager
                .GetCommand<GetScheduleCommand>()
                .Handle(new GetScheduleRequest(UserId.Create(userId).Value, ScheduleId.Create(scheduleId).Value));
            
            return scheduleResult.ToActionResult(schedule => mapper.Map<RepeatsScheduleDto>(schedule));
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

            var scheduleResult = await repeatsScheduleService.PatchSchedule(
                userId.Value, 
                ScheduleId.Create(scheduleId).Value, 
                mapper.Map<UpdateScheduleItem>(request));

            return scheduleResult.ToActionResult(schedule => mapper.Map<RepeatsScheduleDto>(schedule));
        }

        [HttpPost(ApiRoutes.Schedule.Post_CreateSchedule)]
        public async Task<ActionResult<RepeatsScheduleDto>> CreateSchedule([FromBody] CreateScheduleRequest request)
        {
            var userId = HttpContext.GetUserId();

            if (userId.IsFailed)
                return BadRequest();
            
            var scheduleResult = await commandManager
                .GetCommand<CreateScheduleCommand>()
                .Handle(new CreateScheduleCommandRequest(
                    userId.Value,
                    mapper.Map<CreateScheduleProps>(request)));
            
            return scheduleResult.ToActionResult(schedule => mapper.Map<RepeatsScheduleDto>(schedule));
        }
    }
}
