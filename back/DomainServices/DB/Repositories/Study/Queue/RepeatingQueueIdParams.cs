using Domain.Card;
using Domain.Schedule;

namespace DomainServices.DB.Repositories.Study.Queue;

public record RepeatingQueueIdParams(RepeatsSchedule Schedule, Card Card);