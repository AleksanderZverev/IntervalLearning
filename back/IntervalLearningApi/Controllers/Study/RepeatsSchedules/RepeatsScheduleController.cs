using Application.Commands.Schedules.CreateSchedule;
using Application.Commands.Schedules.GetAvailableSchedules;
using Application.Commands.Schedules.GetSchedule;
using Application.Commands.Schedules.UpdateSchedule;
using DB.Models.ValueObjects;
using Domain.User.ValueObjects;
using Infrastructure.Extensions;
using IntervalLearningApi.Constants;
using IntervalLearningApi.Extensions;
using IntervalLearningApi.Infrastructure.CommandManager;
using IntervalLearningApi.Infrastructure.ValidatorResolver;
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
        private readonly ValidatorResolver validatorResolver;
        private readonly IMapper mapper;
        private readonly CommandManager commandManager;

        public RepeatsScheduleController(
            ValidatorResolver validatorResolver,
            IMapper mapper,
            CommandManager commandManager)
        {
            this.validatorResolver = validatorResolver;
            this.mapper = mapper;
            this.commandManager = commandManager;
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
        public async Task<ActionResult<RepeatsScheduleDto>> GetSchedule(
            long userId, 
            short scheduleId)
        {
            var argsResult = (
                userId: UserId.Create(userId),
                scheduleId: ScheduleId.Create(scheduleId)
            );

            if (argsResult.HasAnyError())
                return BadRequest();
            
            var scheduleResult = await commandManager
                .GetCommand<GetScheduleCommand>()
                .Handle(new GetScheduleRequest(
                    argsResult.userId.Value, 
                    argsResult.scheduleId.Value));
            
            return scheduleResult.ToActionResult(schedule => mapper.Map<RepeatsScheduleDto>(schedule));
        }

        [HttpGet(ApiRoutes.Schedule.Get_GetMySchedule)]
        public async Task<ActionResult<RepeatsScheduleDto>> GetSchedule(
            short scheduleId)
        {
            var argsResult = (
                userId: HttpContext.GetUserId(),
                scheduleId: ScheduleId.Create(scheduleId)
            );

            if (argsResult.HasAnyError())
                return BadRequest();

            var scheduleResult = await commandManager
                .GetCommand<GetScheduleCommand>()
                .Handle(new GetScheduleRequest(
                    argsResult.userId.Value, 
                    argsResult.scheduleId.Value));

            return scheduleResult.ToActionResult(schedule => mapper.Map<RepeatsScheduleDto>(schedule));
        }

        [HttpPatch(ApiRoutes.Schedule.Patch_EditSchedule)]
        public async Task<ActionResult<RepeatsScheduleDto>> UpdateSchedule(
            short scheduleId, 
            [FromBody] UpdateScheduleRequest request)
        {
            var validation = validatorResolver.Validate(request);

            if (validation.IsFailed)
                return validation.ToErrorActionResult();
            
            var argsResult = (
                userId: HttpContext.GetUserId(),
                scheduleId: ScheduleId.Create(scheduleId)
            );

            if (argsResult.HasAnyError())
                return BadRequest();

            var scheduleResult = await commandManager
                .GetCommand<UpdateScheduleCommand>()
                .Handle(new UpdateScheduleCommandRequest(
                    argsResult.userId.Value,
                    argsResult.scheduleId.Value,
                    mapper.Map<UpdateScheduleProps>(request)));

            return scheduleResult.ToActionResult(schedule => mapper.Map<RepeatsScheduleDto>(schedule));
        }

        [HttpPost(ApiRoutes.Schedule.Post_CreateSchedule)]
        public async Task<ActionResult<RepeatsScheduleDto>> CreateSchedule(
            [FromBody] CreateScheduleRequest request)
        {
            var validation = validatorResolver.Validate(request);

            if (validation.IsFailed)
                return validation.ToErrorActionResult();
            
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
