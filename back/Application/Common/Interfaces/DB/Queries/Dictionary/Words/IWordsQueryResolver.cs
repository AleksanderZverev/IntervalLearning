using DB.Models.Dictionary;
using DB.Models.Dictionary.ValueObjects;
using Domain.Language.ValueObjects;

namespace Application.Common.Interfaces.DB.Queries.Dictionary.Words;

public interface IWordsQueryResolver
{
    Task<List<LanguageWord>> GetAll(LanguageId languageId);
    Task<List<LanguageWord>> SearchWord(WordText text, int count);
    Task<List<LanguageWord>> SearchWordByPronunciation(WordText text, int count);
}