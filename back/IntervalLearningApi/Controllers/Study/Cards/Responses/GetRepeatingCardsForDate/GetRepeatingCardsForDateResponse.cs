using Application.Commands.Cards.GetCardsQueueCommand;
using Domain.Card;
using IntervalLearningApi.Controllers.Study.Cards.DTOs;
using Mapster;

namespace IntervalLearningApi.Controllers.Study.Cards.Responses.GetRepeatingCardsForDate;

public record GetRepeatingCardsForDateResponse(
    List<CardDto> Cards,
    int TotalCardsCount);
    
    
public class GetRepeatingCardsForDateResponseRegister : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<Card, CardDto>();
        config.NewConfig<GetCardsQueueCommandResponse, GetRepeatingCardsForDateResponse>();
    }
}