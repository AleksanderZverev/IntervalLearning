using Application.Common.Interfaces.DB.Queries.Dictionary.Languages;
using Application.Common.Interfaces.DB.Queries.Dictionary.Words;
using Application.Common.Interfaces.DB.Repositories;

namespace Application.Common.Interfaces.DB.Queries.Dictionary;

public interface IDictionaryQueryRepository : IBoundedContextQueryRepository
{
    public ILanguagesQueryResolver  Languages { get; }
    public IWordsQueryResolver Words { get; }
}