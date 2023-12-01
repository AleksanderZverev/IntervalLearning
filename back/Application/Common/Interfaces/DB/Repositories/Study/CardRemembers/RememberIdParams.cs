using Domain.Card;
using Domain.Schedule;

namespace Application.Common.Interfaces.DB.Repositories.Study.CardRemembers;

public record RememberIdParams(RepeatsSchedule Schedule, Card Card);
