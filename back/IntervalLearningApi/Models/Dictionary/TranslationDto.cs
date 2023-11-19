using DB.Models.Dictionary;
using Mapster;

namespace IntervalLearningApi.Models.Dictionary;

public class TranslationRegister : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<WordTranslation, TranslationDto>();
    }
}

public class TranslationDto
{
    public string LanguageId { get; set; }
    public string Id { get; set; }
    public string Translation { get; set; }
}