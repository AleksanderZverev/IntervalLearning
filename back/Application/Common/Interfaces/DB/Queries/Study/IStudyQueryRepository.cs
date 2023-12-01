using Application.Common.Interfaces.DB.Queries.Study.Cards;
using Application.Common.Interfaces.DB.Queries.Study.Collections;
using Application.Common.Interfaces.DB.Queries.Study.Queue;
using Application.Common.Interfaces.DB.Queries.Study.Remember;
using Application.Common.Interfaces.DB.Queries.Study.Schedule;
using Application.Common.Interfaces.DB.Queries.Study.Themes;
using Application.Common.Interfaces.DB.Repositories;

namespace Application.Common.Interfaces.DB.Queries.Study;

public interface IStudyQueryRepository : IBoundedContextRepository
{
    public ICardsQueryResolver Cards { get; }
    public ICollectionQueryResolver Collections { get; }
    public IRepeatingQueueResolver RepeatingQueue { get; }
    public IRememberQueryResolver CardRemembers { get; }
    public IScheduleResolver Schedules { get; }
    public IThemesQueryResolver Themes { get; }
}