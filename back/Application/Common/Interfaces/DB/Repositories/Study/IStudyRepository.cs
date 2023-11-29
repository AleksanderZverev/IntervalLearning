using Application.Common.Interfaces.DB.Repositories.Cards;
using Application.Common.Interfaces.DB.Repositories.Study.CardRemembers;
using Application.Common.Interfaces.DB.Repositories.Study.Collections;
using Application.Common.Interfaces.DB.Repositories.Study.Queue;
using Application.Common.Interfaces.DB.Repositories.Study.Themes;
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

namespace Application.Common.Interfaces.DB.Repositories.Study;

public interface IStudyRepository : IBoundedContextRepository
{
    public IStudyQueryRepository Query { get; }
    
    public IRepository<Theme, ThemeId, ThemeIdParams> Themes { get; }
    
    public IRepository<Collection, CollectionId, CollectionIdParams> Collections { get; }
    public IRepository<Card, CardId, CardIdParams> Cards { get; }
    
    public IRepository<RepeatsSchedule> RepeatsSchedules { get; }
    public IRepository<Phase> Phases { get; }
    
    public IRepository<CardRepeatQueue, QueueId, RepeatingQueueIdParams> RepeatingQueue { get; }
    
    public IRepository<Remember, RememberId, RememberIdParams> CardRemembers { get; }
    public IRepository<PhaseRememberEntity> PhaseRemembers { get; }
}