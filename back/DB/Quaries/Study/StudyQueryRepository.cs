using Application.Common.Interfaces.DB.Queries.Study;
using Application.Common.Interfaces.DB.Queries.Study.Cards;
using Application.Common.Interfaces.DB.Queries.Study.Collections;
using Application.Common.Interfaces.DB.Queries.Study.Queue;
using Application.Common.Interfaces.DB.Queries.Study.Remember;
using Application.Common.Interfaces.DB.Queries.Study.Schedule;
using Application.Common.Interfaces.DB.Queries.Study.Themes;

namespace DB.Quaries.Study;

public class StudyQueryRepository : IStudyQueryRepository
{
    public ICardsQueryResolver Cards { get; }
    public ICollectionQueryResolver Collections { get; }
    public IRepeatingQueueResolver RepeatingQueue { get; }
    public IRememberQueryResolver CardRemembers { get; }
    public IScheduleResolver Schedules { get; }
    public IThemesQueryResolver Themes { get; }
    
    public StudyQueryRepository(ICardsQueryResolver cards,
        ICollectionQueryResolver collections,
        IRepeatingQueueResolver repeatingQueue,
        IRememberQueryResolver cardRemembers,
        IScheduleResolver schedules,
        IThemesQueryResolver themes)
    {
        Cards = cards;
        Collections = collections;
        RepeatingQueue = repeatingQueue;
        CardRemembers = cardRemembers;
        Schedules = schedules;
        Themes = themes;
    }
}