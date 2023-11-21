using System.ComponentModel.DataAnnotations;
using DB;
using DB.Configurations.Study;
using DB.Models;
using DB.Models.ValueObjects;
using Domain.Schedule;
using Domain.Schedule.ValueObjects;
using Domain.User.ValueObjects;
using FluentResults;
using Infrastructure.Errors;
using Microsoft.EntityFrameworkCore;

namespace IntervalLearningApi.Services;

public class RepeatsScheduleService
{
    private readonly ApplicationContext db;

    public RepeatsScheduleService(ApplicationContext db)
    {
        this.db = db;
    }

    public List<RepeatsSchedule> GetAll(UserId userId) 
        => db.RepeatsSchedules
            .Where(s => s.ParentUserId == userId || s.IsRecommended)
            .Include(s => s.Phases)
            .AsSplitQuery()
            .ToList();

    public async Task<Result<RepeatsSchedule>> PatchSchedule(
        UserId userId,
        ScheduleId scheduleId,
        UpdateScheduleItem item)
    {
        var schedule = Find(userId, scheduleId);

        if (schedule == null)
            return new NotFoundError("Schedule");

        await using var transaction = await db.Database.BeginTransactionAsync();

        schedule.Title = item.Title;
        schedule.CardsCountPerPhase = item.CardsCountPerPhase;

        schedule.ShortDescription = item.ShortDescription;
        schedule.DefaultPhaseShortDescription = item.DefaultPhaseShortDescription;
        schedule.DefaultRepeatPhaseShortDescription = item.DefaultRepeatPhaseShortDescription;

        schedule.OnStartLearningDescription = item.OnStartLearningDescription;
        schedule.DefaultPhaseDescription = item.DefaultPhaseDescription;
        schedule.DefaultRepeatPhaseDescription = item.DefaultRepeatPhaseDescription;

        db.Update(schedule);

        if (item.Phases != null)
        {
            db.Phases.RemoveRange(schedule.Phases);
            schedule.Phases = item.Phases.Select(p => ConvertToPhase(schedule, p)).ToList();
            db.Phases.AddRange(schedule.Phases);
        }

        if (!await db.SoftSaveChangesAsync())
        {
            return new InternalError();
        }
        
        await transaction.CommitAsync();
        return schedule;
    }

    public async Task<Result<RepeatsSchedule>> Create(
        UserId userId, 
        CreateScheduleItem item)
    {
        await using var transaction = await db.Database.BeginTransactionAsync();

        var seqName = RepeatScheduleConfiguration.GetSequenceName(userId);
        db.EnsureSequenceCreated(seqName);
        var nextId = db.GetSequenceNextValue16(seqName);
        var scheduleId = ScheduleId.Create(nextId).Value;
        
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
        
        db.RepeatsSchedules.Add(newSchedule);

        if (!await db.SoftSaveChangesAsync())
        {
            return new InternalError();
        }

        await transaction.CommitAsync();
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

    public RepeatsSchedule? Find(UserId userId, ScheduleId scheduleId)
    {
        return db.RepeatsSchedules
            .Include(s => s.Phases)
            .AsSplitQuery()
            .SingleOrDefault(s => s.ParentUserId == userId && s.Id == scheduleId);
    }
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

public class UpdateScheduleItem : BaseRepeatsScheduleItem
{
    public List<UpdatePhaseInfo> Phases { get; set; }
}

public class CreateScheduleItem : BaseRepeatsScheduleItem
{
    public ForgottenBehavior ForgottenBehavior { get; set; }
    public List<PhaseInfo> Phases { get; set; }
}

public class UpdatePhaseInfo : PhaseInfo
{
}

public class PhaseInfo
{
    public PhaseId Id { get; set; }
    public uint SecondsFromLastPhase { get; set; }
    public LongSingleLineString? ShortDescription { get; set; }
    public LongMultiLineString? Description { get; set; }
    public bool IsDefaultValueSide { get; set; }
}
