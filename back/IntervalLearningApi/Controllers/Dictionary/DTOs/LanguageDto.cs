using Domain.Language;
using Mapster;

namespace IntervalLearningApi.Models.Dictionary;

public class LanguageRegister : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<Language, LanguageDto>()
            .Map(d => d.Id, s => s.Id.Value.ToString());
    }
}

public class LanguageDto
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string NativeLanguageName { get; set; }
    public string? TranslationLinkTitle { get; set; }
    public string? TranslationLink { get; set; }
}