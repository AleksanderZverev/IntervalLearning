using DB.Models.ValueObjects;
using Domain.Card;
using Domain.Schedule;
using FluentResults;

namespace Application.Common.Interfaces.DB.Repositories.Study.CardRemembers;

public record RememberIdParams(RepeatsSchedule Schedule, Card Card);
