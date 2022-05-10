using System.ComponentModel.DataAnnotations;
using DB;
using DB.Models;
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

    public async Task<(RepeatsScheduleEntity? schedule, string? error)> Create(
        long userId, 
        RepeatsScheduleController.CreateScheduleRequest request)
    {
        db.Database.BeginTransaction();

        var schedule = await scheduleRep.Create(new CreateScheduleItem(
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
            db.Database.RollbackTransaction();
            return (null, "Unable to create schedule");
        }

        var phaseEntities = request.Phases
            .Select(p => phasesRep.Create(
                new CreatePhaseItem(
                    userId,
                    p.Id,
                    schedule.Id,
                    p.SecondsFromLastPhase,
                    p.ShortDescription,
                    p.Description,
                    p.IsDefaultValueSide)))
            .ToList();

        try
        {
            await db.SaveChangesAsync();
            db.Database.CommitTransaction();
            return (schedule, null);
        }
        catch
        {
            db.Database.RollbackTransaction();
            return (null, "Unable to create phases");
        }
    }
}

public class PhaseInfo
{
    [Required]
    public byte Id { get; set; }

    [Required]
    public uint SecondsFromLastPhase { get; set; }
    [StringLength(100)]
    public string? ShortDescription { get; set; }

    [StringLength(1000)]
    public string? Description { get; set; }

    public bool IsDefaultValueSide { get; set; }
}