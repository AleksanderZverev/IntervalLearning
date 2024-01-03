using DomainServices.DB.Queries.Dictionary;
using DomainServices.DB.Queries.Dictionary.Languages;
using DomainServices.DB.Queries.Dictionary.Words;

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