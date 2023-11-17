using System.ComponentModel.DataAnnotations;
using DB;
using DB.Configurations.Study;
using DB.Models;
using DB.Models.ValueObjects;
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

        try
        {
            db.RepeatsSchedules.Add(newSchedule);
            await db.SaveChangesAsync();
        }
        catch
        {
            await db.Database.RollbackTransactionAsync();
            return (null, "Unable to create schedule");
        }

        //TODO: Upper in one operation
        var phases = item.Phases
            .Select(phase => db.CreateByProperties<PhaseEntity>(
                new CreatePhaseItem(
                    userId,
                    phase.Id,
                    newSchedule.Id,
                    phase.SecondsFromLastPhase,
                    phase.ShortDescription,
                    phase.Description,
                    phase.IsDefaultValueSide)))
            .ToList();

        try
        {
            await db.SaveChangesAsync();
            await db.Database.CommitTransactionAsync();
            
            newSchedule.Phases = phases;
            return (newSchedule, null);
        }
        catch
        {
            await db.Database.RollbackTransactionAsync();
            return (null, "Unable to create phases");
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
    public ScheduleShortDescription? ShortDescription { get; set; }
    public ScheduleLongDescription? OnStartLearningDescription { get; set; }
    public short CardsCountPerPhase { get; set; }
    public ScheduleShortDescription? DefaultPhaseShortDescription { get; set; }
    public ScheduleLongDescription? DefaultPhaseDescription { get; set; }
    public ScheduleShortDescription? DefaultRepeatPhaseShortDescription { get; set; }
    public ScheduleLongDescription? DefaultRepeatPhaseDescription { get; set; }
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

public class UpdatePhaseInfo
{
    [Required]
    public short Id { get; set; }

    [StringLength(PhaseEntity.ShortDescriptionLength)]
    public string? ShortDescription { get; set; }

    [StringLength(1000)]
    public string? Description { get; set; }

    public bool IsDefaultValueSide { get; set; }
}

public class PhaseInfo : UpdatePhaseInfo
{
    [Required]
    public uint SecondsFromLastPhase { get; set; }
}
