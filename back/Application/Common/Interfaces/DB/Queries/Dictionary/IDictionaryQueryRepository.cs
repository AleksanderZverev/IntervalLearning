using Application.Common.Interfaces.Domain.Dictionary.Words;
using Application.Common.Interfaces.Domain.Languages;

namespace Application.Common.Interfaces.DB.Queries.Dictionary;

public interface IDictionaryQueryRepository
{
    public ILanguagesQueryResolver  Languages { get; }
    public IWordsQueryResolver Words { get; }
}