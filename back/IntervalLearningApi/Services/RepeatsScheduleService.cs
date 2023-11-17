using System.ComponentModel.DataAnnotations;
using DB;
using DB.Configurations.Study;
using DB.Models;
using DB.Models.ValueObjects;
using Domain.Schedule;
using Domain.User.ValueObjects;
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

    public async Task<(RepeatsSchedule? schedule, string? error)> PatchSchedule(
        UserId userId, 
        ScheduleId scheduleId,
        UpdateScheduleItem item)
    {
        var schedule = Find(userId, scheduleId);

        if (schedule == null)
            return (null, "not found");
            
        await db.Database.BeginTransactionAsync();
        
        schedule.Title = item.Title;
        schedule.CardsCountPerPhase = item.CardsCountPerPhase;
         
        schedule.ShortDescription = item.ShortDescription;
        schedule.DefaultPhaseShortDescription = item.DefaultPhaseShortDescription;
        schedule.DefaultRepeatPhaseShortDescription = item.DefaultRepeatPhaseShortDescription;
        
        schedule.OnStartLearningDescription = item.OnStartLearningDescription;
        schedule.DefaultPhaseDescription = item.DefaultPhaseDescription;
        schedule.DefaultRepeatPhaseDescription = item.DefaultRepeatPhaseDescription;

        try
        {
            db.Update(schedule);
            await db.SaveChangesAsync();
        }
        catch
        {
            await db.Database.RollbackTransactionAsync();
            return (null, "Unable to edit schedule");
        }

        //TODO: move upper
        if (item.Phases is not {Count: >0})
        {
            await db.Database.CommitTransactionAsync();
            return (schedule, null);
        }

        foreach (var updateItem in item.Phases)
        {
            var phaseEntity = schedule.Phases.SingleOrDefault(p => p.Id == updateItem.Id);

            if (phaseEntity == null)
            {
                await db.Database.RollbackTransactionAsync();
                return (null, "Phase not found");
            }

            db.Entry(phaseEntity).CurrentValues.SetValues(updateItem);
            phaseEntity.OnLearnDescription = updateItem.Description;
        }

        try
        {
            await db.SaveChangesAsync();
            await db.Database.CommitTransactionAsync();
            return (schedule, null);
        }
        catch
        {
            await db.Database.RollbackTransactionAsync();
            return (null, "Unable to create phases");
        }
    }

    public async Task<(RepeatsSchedule? schedule, string? error)> Create(
        UserId userId, 
        CreateScheduleItem item)
    {
        await db.Database.BeginTransactionAsync();

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

        var phaseSeqName = PhaseConfiguration.GetSequenceName(newSchedule.ParentUserId, newSchedule.Id);
        db.EnsureSequenceCreated(phaseSeqName);
        
        var newPhases = item.Phases.Select(p =>
        {
            var nextPhaseId = db.GetSequenceNextValue16(phaseSeqName);
            var phaseId = PhaseId.Create(nextPhaseId).Value;
            return new Phase(newSchedule.Id, newSchedule.ParentUserId, phaseId)
            {
                SecondsFromLastPhase = p.SecondsFromLastPhase,
                IsDefaultValueSide = p.IsDefaultValueSide,
                ShortDescription = p.ShortDescription,
                OnLearnDescription = p.Description,
            };
        }).ToList();
        db.Phases.UpdateRange(newPhases);

        try
        {
            db.RepeatsSchedules.Add(newSchedule);
            await db.SaveChangesAsync();
            
            newSchedule.Phases = newPhases;
            return (newSchedule, null);
        }
        catch (Exception e)
        {
            await db.Database.RollbackTransactionAsync();
            return (null, "Unable to create schedule");
        }
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
    [Required]
    public PhaseId Id { get; init; }
}

public class PhaseInfo
{
    [Required]
    public uint SecondsFromLastPhase { get; init; }

    [StringLength(200)]
    public LongSingleLineString? ShortDescription { get; init; }

    [StringLength(1000)]
    public LongMultiLineString? Description { get; init; }
    
    public bool IsDefaultValueSide { get; init; }
}
