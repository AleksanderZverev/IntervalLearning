using System.ComponentModel.DataAnnotations;
using DB;
using DB.Models;
using Domain.User.ValueObjects;
using IntervalLearningApi.Controllers;
using Microsoft.EntityFrameworkCore;

namespace IntervalLearningApi.Services;

public class RepeatsScheduleService
{
    private readonly Repository<PhaseEntity> phasesRep;
    private readonly Repository<RepeatsScheduleEntity> scheduleRep;
    private readonly ApplicationContext db;

    public RepeatsScheduleService(
        Repository<PhaseEntity> phasesRep,
        Repository<RepeatsScheduleEntity> scheduleRep,
        ApplicationContext db)
    {
        this.phasesRep = phasesRep;
        this.scheduleRep = scheduleRep;
        this.db = db;
    }

    public List<RepeatsScheduleEntity> GetAll(long userId) 
        => db.RepeatsSchedules
            .Where(s => s.ParentUserId == userId || s.IsRecommended)
            .Include(s => s.Phases)
            .AsSplitQuery()
            .ToList();

    public async Task<(RepeatsScheduleEntity? schedule, string? error)> PatchSchedule(
        UserId userId, 
        short scheduleId,
        RepeatsScheduleController.UpdateScheduleRequest request)
    {
        var originSchedule = Find(userId, scheduleId);

        if (originSchedule == null)
            return (null, "not found");
            
        await db.Database.BeginTransactionAsync();

        var schedule = db.UpdateByProperties<RepeatsScheduleEntity>(new PatchRepeatsSchedule(
            request.CardsCountPerPhase,
            request.Title,
            request.ShortDescription,
            request.Description,
            request.DefaultPhaseShortDescription,
            request.DefaultPhaseDescription,
            request.DefaultRepeatPhaseShortDescription,
            request.DefaultRepeatPhaseDescription
        ), userId, scheduleId);

        try
        {
            await db.SaveChangesAsync();
        }
        catch
        {
            await db.Database.RollbackTransactionAsync();
            return (null, "Unable to edit schedule");
        }

        if (request.Phases == null || request.Phases.Count == 0)
        {
            await db.SaveChangesAsync();
            await db.Database.CommitTransactionAsync();
            return (schedule, null);
        }

        foreach (var updateItem in request.Phases)
        {
            var phaseEntity = originSchedule.Phases.SingleOrDefault(p => p.Id == updateItem.Id);

            if (phaseEntity == null)
            {
                await db.Database.RollbackTransactionAsync();
                return (null, "Phase not found");
            }

            db.Entry(phaseEntity).CurrentValues.SetValues(updateItem);
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

    public async Task<(RepeatsScheduleEntity? schedule, string? error)> Create(
        UserId userId, 
        RepeatsScheduleController.CreateScheduleRequest request)
    {
        await db.Database.BeginTransactionAsync();

        var schedule = db.CreateByProperties<RepeatsScheduleEntity>(new CreateScheduleItem(
            userId,
            request.CardsCountPerPhase,
            (ForgottenBehavior)request.ForgottenBehavior,
            request.Title,
            request.ShortDescription,
            request.Description,
            request.DefaultPhaseShortDescription,
            request.DefaultPhaseDescription,
            request.DefaultRepeatPhaseShortDescription,
            request.DefaultRepeatPhaseDescription
        ));

        try
        {
            await db.SaveChangesAsync();
        }
        catch
        {
            await db.Database.RollbackTransactionAsync();
            return (null, "Unable to create schedule");
        }

        var phaseEntities = request.Phases
            .Select(phase => db.CreateByProperties<PhaseEntity>(
                new CreatePhaseItem(
                    userId,
                    phase.Id,
                    schedule.Id,
                    phase.SecondsFromLastPhase,
                    phase.ShortDescription,
                    phase.Description,
                    phase.IsDefaultValueSide)))
            .ToList();

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

    public RepeatsScheduleEntity? Find(UserId userId, short scheduleId)
    {
        return db.RepeatsSchedules
            .Include(s => s.Phases)
            .AsSplitQuery()
            .SingleOrDefault(s => s.ParentUserId == userId && s.Id == scheduleId);
    }
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
