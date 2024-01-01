using Application.Commands.Cards.StartLearnCards;
using Domain.Card.ValueObjects;
using Mapster;

namespace IntervalLearningApi.Controllers.Study.Cards.Requests.StartCards;


public class CardMovementInfoDtoRegister : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<CardMovementInfo, CardMovementInfoDto>();
    }
}

public record CardMovementInfoDto(List<string> CardIds, DateTime NextRepetitionDate);