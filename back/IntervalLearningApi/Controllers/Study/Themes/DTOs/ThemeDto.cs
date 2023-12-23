using Domain.Theme;
using Domain.Theme.ValueObjects;
using Mapster;

namespace IntervalLearningApi.Controllers.Study.Themes.DTOs;

public class ThemeRegister : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<ThemeId, short>()
            .Map(d => d, s => s.Value);

        config.NewConfig<Theme, ThemeDto>()
            .Map(d => d.Id, s => s.Id.Value);
    }
}

public class ThemeDto
{
    public short Id { get; set; }
    public string Name { get; set; }
    public string? LanguageId { get; set; }
}