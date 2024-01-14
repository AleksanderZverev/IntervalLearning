using Domain.Dictionary.Word;
using Domain.Dictionary.Word.ValueObjects;
using Domain.Language.ValueObjects;

namespace DomainServices.DB.Queries.Dictionary.Words;

public interface IWordsQueryResolver
{
    Task<List<LanguageWord>> GetAll(LanguageId languageId);
    Task<List<LanguageWord>> SearchWord(WordText text, int count);
    Task<List<LanguageWord>> SearchWordByPronunciation(WordText text, int count);
}