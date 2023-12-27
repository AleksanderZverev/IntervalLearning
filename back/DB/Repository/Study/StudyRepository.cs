using Application.Common.Interfaces.DB.Queries.Study;
using Application.Common.Interfaces.DB.Repositories;
using Application.Common.Interfaces.DB.Repositories.Study;
using Application.Common.Interfaces.DB.Repositories.Study.CardRemembers;
using Application.Common.Interfaces.DB.Repositories.Study.Cards;
using Application.Common.Interfaces.DB.Repositories.Study.Collections;
using Application.Common.Interfaces.DB.Repositories.Study.Queue;
using Application.Common.Interfaces.DB.Repositories.Study.Schedules;
using Application.Common.Interfaces.DB.Repositories.Study.Themes;
using Domain.Card;
using Domain.Card.ValueObjects;
using Domain.Collection;
using Domain.Collection.ValueObjects;
using Domain.Queue;
using Domain.Queue.ValueObjects;
using Domain.RelearningCard;
using Domain.Schedule;
using Domain.Schedule.Entities.Phase;
using Domain.Schedule.Entities.Phase.Entities;
using Domain.Schedule.Entities.Remember;
using Domain.Schedule.Entities.Remember.ValueObjects;
using Domain.Schedule.ValueObjects;
using Domain.Theme;
using Domain.Theme.ValueObjects;
using FluentResults;
using Infrastructure.Errors;

namespace DB.Repository.Study;

public class StudyRepository : IStudyRepository
{
    private readonly ApplicationContext db;
    
    public IStudyQueryRepository Query { get; }
    
    public IRepository<Theme, ThemeId, ThemeIdParams> Themes { get; }
    
    public IRepository<Collection, CollectionId, CollectionIdParams> Collections { get; }
    public IRepository<Card, CardId, CardIdParams> Cards { get; }
    
    public IRepository<RepeatsSchedule, ScheduleId, ScheduleIdParams> RepeatsSchedules { get; }
    public IRepository<Phase> Phases { get; }
    
    public IRepository<CardRepeatQueue, QueueId, RepeatingQueueIdParams> RepeatingQueue { get; }
    
    public IRepository<Remember, RememberId, RememberIdParams> CardRemembers { get; }
    public IRepository<PhaseRememberEntity> PhaseRemembers { get; }
    public IRepository<RelearningCard> RelearnCards { get; }

    public StudyRepository(
        ApplicationContext db,
        IStudyQueryRepository query,
        IRepository<Theme, ThemeId, ThemeIdParams> themes,
        IRepository<Collection, CollectionId, CollectionIdParams> collections,
        IRepository<Card, CardId, CardIdParams> cards,
        IRepository<RepeatsSchedule, ScheduleId, ScheduleIdParams> repeatsSchedules,
        IRepository<Phase> phases,
        IRepository<CardRepeatQueue, QueueId, RepeatingQueueIdParams> repeatingQueue,
        IRepository<Remember, RememberId, RememberIdParams> cardRemembers,
        IRepository<PhaseRememberEntity> phaseRemembers, 
        IRepository<RelearningCard> relearnCards)
    {
        this.db = db;
        Query = query;
        Themes = themes;
        Collections = collections;
        Cards = cards;
        RepeatsSchedules = repeatsSchedules;
        Phases = phases;
        RepeatingQueue = repeatingQueue;
        CardRemembers = cardRemembers;
        PhaseRemembers = phaseRemembers;
        RelearnCards = relearnCards;
    }
    
    public Result SaveChanges()
    {
        return Result.OkIf(db.SoftSaveChanges(), new InternalError());
    }

    public async Task<Result> SaveChangesAsync()
    {
        return Result.OkIf(await db.SoftSaveChangesAsync(), new InternalError());
    }
}