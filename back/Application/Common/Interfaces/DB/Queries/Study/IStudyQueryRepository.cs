using Application.Common.Interfaces.Domain.Cards;
using Application.Common.Interfaces.Domain.Collections;
using Application.Common.Interfaces.Domain.Study.Queue;
using Application.Common.Interfaces.Domain.Study.Remember;
using Application.Common.Interfaces.Domain.Study.Schedule;
using Application.Common.Interfaces.Domain.Themes;

namespace Application.Common.Interfaces.DB.Repositories.Study;

public interface IStudyQueryRepository : IBoundedContextRepository
{
    public ICardsQueryResolver Cards { get; }
    public ICollectionQueryResolver Collections { get; }
    public IRepeatingQueueResolver RepeatingQueue { get; }
    public IRememberQueryResolver CardRemembers { get; }
    public IScheduleResolver Schedules { get; }
    public IThemesQueryResolver Themes { get; }
}