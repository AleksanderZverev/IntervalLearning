using DomainServices.DB.Queries.Study;
using DomainServices.DB.Queries.Study.Cards;
using DomainServices.DB.Queries.Study.Collections;
using DomainServices.DB.Queries.Study.Queue;
using DomainServices.DB.Queries.Study.RelearningCards;
using DomainServices.DB.Queries.Study.Remember;
using DomainServices.DB.Queries.Study.Schedule;
using DomainServices.DB.Queries.Study.Themes;

namespace DB.Quaries.Study;

public class StudyQueryRepository : IStudyQueryRepository
{
    public ICardsQueryResolver Cards { get; }
    public ICollectionQueryResolver Collections { get; }
    public IRepeatingQueueResolver RepeatingQueue { get; }
    public IRememberQueryResolver CardRemembers { get; }
    public IScheduleResolver Schedules { get; }
    public IThemesQueryResolver Themes { get; }
    public IRelearningCardsResolver RelearningCards { get; }

    public StudyQueryRepository(ICardsQueryResolver cards,
        ICollectionQueryResolver collections,
        IRepeatingQueueResolver repeatingQueue,
        IRememberQueryResolver cardRemembers,
        IScheduleResolver schedules,
        IThemesQueryResolver themes, 
        IRelearningCardsResolver relearningCards)
    {
        Cards = cards;
        Collections = collections;
        RepeatingQueue = repeatingQueue;
        CardRemembers = cardRemembers;
        Schedules = schedules;
        Themes = themes;
        RelearningCards = relearningCards;
    }
}