using DB.Models.Dictionary;
using Mapster;

namespace IntervalLearningApi.Controllers.Dictionary.DTOs
{
    public class WordRegister : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            config.NewConfig<LanguageWord, WordDto>();
        }
    }

    public class WordDto
    {
        public string Id { get; set; }
        public string Word { get; set; }
        public string? Pronunciation { get; set; }
        public string LanguageId { get; set; }
    }
}
