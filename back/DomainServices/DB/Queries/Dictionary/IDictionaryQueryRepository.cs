using DomainServices.DB.Queries.Dictionary.Languages;
using DomainServices.DB.Queries.Dictionary.Words;
using DomainServices.DB.Repositories;

namespace DomainServices.DB.Queries.Dictionary;

public interface IDictionaryQueryRepository : IBoundedContextQueryRepository
{
    public ILanguagesQueryResolver  Languages { get; }
    public IWordsQueryResolver Words { get; }
}