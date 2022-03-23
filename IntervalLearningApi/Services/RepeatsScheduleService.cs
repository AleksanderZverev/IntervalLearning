using System.ComponentModel.DataAnnotations;
using DB;
using DB.Models;
using Microsoft.EntityFrameworkCore;

namespace IntervalLearningApi.Services;

public class RepeatsScheduleService
{
    private readonly ApplicationContext db;

    public RepeatsScheduleService(ApplicationContext db)
    {
        this.db = db;
    }

    public List<RepeatsScheduleEntity> GetAll(long userId) 
        => db.RepeatsSchedules
            .Where(s => s.ParentUserId == userId)
            .Include(s => s.Phases)
            .AsNoTracking()
            .ToList();

    public (RepeatsScheduleEntity? schedule, string? error) Create(
        long userId,
        short cardsCountPerPhase,
        ForgottenBehavior forgottenBehavior,
        string title,
        List<PhaseInfo> phases,
        string? description)
    {
        try
        {
            db.Database.BeginTransaction();
            var result = CreateWithoutTransaction(userId, cardsCountPerPhase, forgottenBehavior, title, phases, description);
            db.Database.CommitTransaction();
            return result;
        }
        catch
        {
            db.Database.RollbackTransaction();
            return (null, "Unknown error");
        }
    }

    private (RepeatsScheduleEntity? schedule, string? error) CreateWithoutTransaction(
        long userId, 
        short cardsCountPerPhase, 
        ForgottenBehavior forgottenBehavior, 
        string title,
        List<PhaseInfo> phases, 
        string? description)
    {
        var schedule = new RepeatsScheduleEntity(
            userId,
            cardsCountPerPhase,
            forgottenBehavior,
            title,
            description
        );

        db.Entry(schedule).State = EntityState.Added;
        db.SaveChanges();

        var phaseEntities = phases.Select(p => new PhaseEntity(
            userId,
            p.Id,
            schedule.Id,
            p.SecondsFromLastPhase,
            p.Description)).ToList();

        phaseEntities.ForEach(f => db.Entry(f).State = EntityState.Added);
        db.SaveChanges();

        schedule.Phases = phaseEntities;

        return (schedule, null);
    }
}

public class PhaseInfo
{
    [Required]
    public byte Id { get; set; }

    [Required]
    public uint SecondsFromLastPhase { get; set; }

    [StringLength(150)]
    public string? Description { get; set; }
}