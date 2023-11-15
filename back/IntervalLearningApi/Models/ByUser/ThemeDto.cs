using DB.Models.ValueObjects;
using Mapster;

namespace IntervalLearningApi.Models.ByUser;

public class ThemeRegister : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<ThemeId, short>()
            .Map(d => d, s => s.Value);
    }
}

public class ThemeDto
{
    public short Id { get; set; }
    public string Name { get; set; }
    public string? LanguageId { get; set; }
}