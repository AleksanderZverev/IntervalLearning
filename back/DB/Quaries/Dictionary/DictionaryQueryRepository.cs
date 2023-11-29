using Application.Common.Interfaces.DB.Queries.Dictionary;
using Application.Common.Interfaces.Domain.Dictionary.Words;
using Application.Common.Interfaces.Domain.Languages;

namespace DB.Quaries.Dictionary;

public class DictionaryQueryRepository : IDictionaryQueryRepository
{
    public ILanguagesQueryResolver Languages { get; }
    public IWordsQueryResolver Words { get; }
    
    public DictionaryQueryRepository(
        ILanguagesQueryResolver languagesQueryResolver,
        IWordsQueryResolver wordsQueryResolver)
    {
        Languages = languagesQueryResolver;
        Words = wordsQueryResolver;
    }
}