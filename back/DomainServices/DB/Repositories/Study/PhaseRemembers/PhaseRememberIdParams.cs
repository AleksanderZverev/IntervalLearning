using Domain.Schedule.Entities.Phase;
using Domain.User.ValueObjects;

namespace DomainServices.DB.Repositories.Study.PhaseRemembers;

public record PhaseRememberIdParams(Phase Phase, UserId RepeatedUserId);