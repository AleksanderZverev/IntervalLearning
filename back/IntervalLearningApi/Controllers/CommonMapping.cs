using Domain.Common.ValueObjects;
using Mapster;

namespace IntervalLearningApi.Controllers;

public class CommonMapping : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<SingleValueObject<string>, string>()
            .Map(_ => _, s => s.Value);
    }
}