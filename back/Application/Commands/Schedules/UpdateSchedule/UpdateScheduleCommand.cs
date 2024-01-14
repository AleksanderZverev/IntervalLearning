using Application.Commands.Schedules.CreateSchedule;
using Domain.Schedule;
using Domain.Schedule.Entities.Phase;
using Domain.Schedule.Entities.Phase.ValueObjects;
using DomainServices.DB.Repositories.Study;
using DomainServices.DB.Transactions;
using FluentResults;
using GlobalTools.Errors;

namespace Application.Commands.Schedules.UpdateSchedule;

public class UpdateScheduleCommand : ICommand<UpdateScheduleCommandRequest, RepeatsSchedule>
{
    private readonly ITransactionProvider transactionProvider;
    private readonly IStudyRepository studyRepository;

    public UpdateScheduleCommand(
        ITransactionProvider transactionProvider,
        IStudyRepository studyRepository)
    {
        this.transactionProvider = transactionProvider;
        this.studyRepository = studyRepository;
    }

    public async Task<Result<RepeatsSchedule>> Handle(UpdateScheduleCommandRequest request)
    {
        var (userId, scheduleId, item) = request;
        var schedule = await studyRepository.Query.Schedules.Find(userId, scheduleId);

        if (schedule == null)
            return new NotFoundError("Schedule");

        using var transaction = transactionProvider.CreateScope();

        schedule.Title = item.Title;
        schedule.CardsCountPerPhase = item.CardsCountPerPhase;

        schedule.ShortDescription = item.ShortDescription;
        schedule.DefaultPhaseShortDescription = item.DefaultPhaseShortDescription;
        schedule.DefaultRepeatPhaseShortDescription = item.DefaultRepeatPhaseShortDescription;

        schedule.OnStartLearningDescription = item.OnStartLearningDescription;
        schedule.DefaultPhaseDescription = item.DefaultPhaseDescription;
        schedule.DefaultRepeatPhaseDescription = item.DefaultRepeatPhaseDescription;

        studyRepository.RepeatsSchedules.Update(schedule);

        if (item.Phases != null)
        {
            studyRepository.Phases.DeleteRange(schedule.Phases);
            
            schedule.Phases = item.Phases.Select(p => ConvertToPhase(schedule, p)).ToList();
            studyRepository.Phases.AddRange(schedule.Phases);
        }

        var updateScheduleResult = await studyRepository.SaveChangesAsync();
        if (updateScheduleResult.IsFailed)
            return updateScheduleResult;
        
        transaction.Complete();
        return schedule;
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