using Application.Common.Interfaces.DB.Repositories;
using Application.Common.Interfaces.DB.Repositories.Cards;
using Application.Common.Interfaces.DB.Repositories.Study;
using Application.Common.Interfaces.DB.Repositories.Study.CardRemembers;
using Application.Common.Interfaces.DB.Repositories.Study.Collections;
using Application.Common.Interfaces.DB.Repositories.Study.Queue;
using DB.Models;
using DB.Models.ValueObjects;
using Domain.Card;
using Domain.Card.ValueObjects;
using Domain.Collection;
using Domain.Collection.ValueObjects;
using Domain.Queue;
using Domain.Schedule;
using Domain.Schedule.Entities.Remember;
using Domain.Theme;
using Domain.User.ValueObjects;

namespace DB.Repository.Study;

public class StudyRepository : IStudyRepository
{
    public IStudyQueryRepository Query { get; }
    
    public IRepository<Theme> Themes { get; }
    
    public IRepository<Collection, CollectionId, CollectionIdParams> Collections { get; }
    public IRepository<Card, CardId, CardIdParams> Cards { get; }
    
    public IRepository<RepeatsSchedule> RepeatsSchedules { get; }
    public IRepository<Phase> Phases { get; }
    
    public IRepository<CardRepeatQueue, QueueId, RepeatingQueueIdParams> RepeatingQueue { get; }
    
    public IRepository<Remember, RememberId, RememberIdParams> CardRemembers { get; }
    public IRepository<PhaseRememberEntity> PhaseRemembers { get; }

    public StudyRepository(
        IStudyQueryRepository query,
        IRepository<Theme> themes,
        IRepository<Collection, CollectionId, CollectionIdParams> collections,
        IRepository<Card, CardId, CardIdParams> cards,
        IRepository<RepeatsSchedule> repeatsSchedules,
        IRepository<Phase> phases,
        IRepository<CardRepeatQueue, QueueId, RepeatingQueueIdParams> repeatingQueue,
        IRepository<Remember, RememberId, RememberIdParams> cardRemembers,
        IRepository<PhaseRememberEntity> phaseRemembers)
    {
        Query = query;
        Themes = themes;
        Collections = collections;
        Cards = cards;
        RepeatsSchedules = repeatsSchedules;
        Phases = phases;
        RepeatingQueue = repeatingQueue;
        CardRemembers = cardRemembers;
        PhaseRemembers = phaseRemembers;
    }
}