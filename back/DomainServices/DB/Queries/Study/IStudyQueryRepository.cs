using DomainServices.DB.Queries.Study.Cards;
using DomainServices.DB.Queries.Study.Collections;
using DomainServices.DB.Queries.Study.Queue;
using DomainServices.DB.Queries.Study.RelearningCards;
using DomainServices.DB.Queries.Study.Remember;
using DomainServices.DB.Queries.Study.Schedule;
using DomainServices.DB.Queries.Study.Themes;
using DomainServices.DB.Repositories;

namespace DomainServices.DB.Queries.Study;

public interface IStudyQueryRepository : IBoundedContextQueryRepository
{
    public ICardsQueryResolver Cards { get; }
    public ICollectionQueryResolver Collections { get; }
    public IRepeatingQueueResolver RepeatingQueue { get; }
    public IRememberQueryResolver CardRemembers { get; }
    public IScheduleResolver Schedules { get; }
    public IThemesQueryResolver Themes { get; }
    public IRelearningCardsResolver RelearningCards { get; }
}