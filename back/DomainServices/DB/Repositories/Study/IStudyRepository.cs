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
using DomainServices.DB.Queries.Study;
using DomainServices.DB.Repositories.Study.CardRemembers;
using DomainServices.DB.Repositories.Study.Cards;
using DomainServices.DB.Repositories.Study.Collections;
using DomainServices.DB.Repositories.Study.PhaseRemembers;
using DomainServices.DB.Repositories.Study.Queue;
using DomainServices.DB.Repositories.Study.Schedules;
using DomainServices.DB.Repositories.Study.Themes;

namespace DomainServices.DB.Repositories.Study;

public interface IStudyRepository : IBoundedContextRepository
{
    public IStudyQueryRepository Query { get; }
    
    public IRepository<Theme, ThemeId, ThemeIdParams> Themes { get; }
    
    public IRepository<Collection, CollectionId, CollectionIdParams> Collections { get; }
    public IRepository<Card, CardId, CardIdParams> Cards { get; }
    
    public IRepository<RepeatsSchedule, ScheduleId, ScheduleIdParams> RepeatsSchedules { get; }
    public IRepository<Phase> Phases { get; }
    
    public IRepository<CardRepeatQueue, QueueId, RepeatingQueueIdParams> RepeatingQueue { get; }
    
    public IRepository<Remember, RememberId, RememberIdParams> CardRemembers { get; }
    public IRepository<PhaseRememberEntity, int, PhaseRememberIdParams> PhaseRemembers { get; }
    public IRepository<RelearningCard> RelearnCards { get; }
}