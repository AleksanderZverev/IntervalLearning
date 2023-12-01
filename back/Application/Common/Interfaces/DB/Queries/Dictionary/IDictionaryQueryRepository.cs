using Application.Common.Interfaces.DB.Queries.Dictionary.Languages;
using Application.Common.Interfaces.DB.Queries.Dictionary.Words;

namespace Application.Common.Interfaces.DB.Queries.Dictionary;

public interface IDictionaryQueryRepository
{
    public ILanguagesQueryResolver  Languages { get; }
    public IWordsQueryResolver Words { get; }
}