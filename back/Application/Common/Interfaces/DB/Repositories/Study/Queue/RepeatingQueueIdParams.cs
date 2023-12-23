using Domain.Card;
using Domain.Schedule;

namespace Application.Common.Interfaces.DB.Repositories.Study.Queue;

public record RepeatingQueueIdParams(RepeatsSchedule Schedule, Card Card);