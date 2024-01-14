using Domain.Card;
using Domain.Schedule;

namespace DomainServices.DB.Repositories.Study.CardRemembers;

public record RememberIdParams(RepeatsSchedule Schedule, Card Card);
