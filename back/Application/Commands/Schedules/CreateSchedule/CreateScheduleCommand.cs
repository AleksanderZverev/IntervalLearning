using Application.Common.Interfaces.DB.Repositories.Study;
using Application.Common.Interfaces.DB.Repositories.Study.Schedules;
using Application.Common.Interfaces.DB.Transactions;
using DB.Models;
using DB.Models.ValueObjects;
using Domain.Schedule;
using Domain.Schedule.ValueObjects;
using Domain.User.ValueObjects;
using FluentResults;
using Infrastructure.Errors;

namespace Application.Commands.Schedules.CreateSchedule;

public record CreateScheduleCommandRequest(UserId UserId, CreateScheduleProps ScheduleProps);

public class CreateScheduleProps : BaseRepeatsScheduleItem
{
    public ForgottenBehavior ForgottenBehavior { get; set; }
    public List<PhaseInfo> Phases { get; set; }
}

public class PhaseInfo
{
    public PhaseId Id { get; set; }
    public uint SecondsFromLastPhase { get; set; }
    public LongSingleLineString? ShortDescription { get; set; }
    public LongMultiLineString? Description { get; set; }
    public bool IsDefaultValueSide { get; set; }
}

public abstract class BaseRepeatsScheduleItem
{
    public required ScheduleTitle Title { get; set; }
    public LongSingleLineString? ShortDescription { get; set; }
    public LongMultiLineString? OnStartLearningDescription { get; set; }
    public short CardsCountPerPhase { get; set; }
    public LongSingleLineString? DefaultPhaseShortDescription { get; set; }
    public LongMultiLineString? DefaultPhaseDescription { get; set; }
    public LongSingleLineString? DefaultRepeatPhaseShortDescription { get; set; }
    public LongMultiLineString? DefaultRepeatPhaseDescription { get; set; }
}

public class CreateScheduleCommand : ICommand<CreateScheduleCommandRequest, RepeatsSchedule>
{
    private readonly ITransactionProvider transactionProvider;
    private readonly IStudyRepository studyRepository;

    public CreateScheduleCommand(
        ITransactionProvider transactionProvider,
        IStudyRepository studyRepository)
    {
        this.transactionProvider = transactionProvider;
        this.studyRepository = studyRepository;
    }

    public async Task<Result<RepeatsSchedule>> Handle(CreateScheduleCommandRequest request)
    {
        using var transaction = transactionProvider.CreateScope();

        var (userId, item) = request;
        var scheduleIdResult = studyRepository.RepeatsSchedules.GetUniqueId(new ScheduleIdParams(userId));

        if (scheduleIdResult.IsFailed)
            return scheduleIdResult.ToResult();

        var scheduleId = scheduleIdResult.Value;
        var newSchedule = new RepeatsSchedule(userId, scheduleId)
        {
            Title = item.Title,
            ForgottenBehavior = item.ForgottenBehavior, // (ForgottenBehavior)request.ForgottenBehavior,
            CardsCountPerPhase = item.CardsCountPerPhase,
            ShortDescription = item.ShortDescription,
            OnStartLearningDescription = item.OnStartLearningDescription, // request.Description,
            DefaultPhaseShortDescription = item.DefaultPhaseShortDescription,
            DefaultPhaseDescription = item.DefaultPhaseDescription,
            DefaultRepeatPhaseShortDescription = item.DefaultRepeatPhaseShortDescription,
            DefaultRepeatPhaseDescription = item.DefaultRepeatPhaseDescription,
        };

        var newPhases = item.Phases.Select(p => ConvertToPhase(newSchedule, p)).ToList();
        newSchedule.Phases = newPhases;

        var addScheduleResult = studyRepository.RepeatsSchedules.AddAndSave(newSchedule);

        if (addScheduleResult.IsFailed)
        {
            return new InternalError();
        }

        transaction.Complete();
        return newSchedule;
    }
    
    private static Phase ConvertToPhase(RepeatsSchedule newSchedule, PhaseInfo phase)
    {
        var phaseId = PhaseId.Create(phase.Id).Value;
        return new Phase(newSchedule.Id, newSchedule.ParentUserId, phaseId)
        {
            SecondsFromLastPhase = phase.SecondsFromLastPhase,
            IsDefaultValueSide = phase.IsDefaultValueSide,
            ShortDescription = phase.ShortDescription,
            OnLearnDescription = phase.Description,
        };
    }
}

