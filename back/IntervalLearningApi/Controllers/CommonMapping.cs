using Domain.Common.ValueObjects;
using Mapster;

namespace IntervalLearningApi.Models;

public class CommonMapping : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<SingleValueObject<string>, string>()
            .Map(_ => _, s => s.Value);
    }
}