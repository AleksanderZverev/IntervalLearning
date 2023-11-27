using DB.Models.Dictionary;
using Domain.Language.ValueObjects;

namespace Application.Common.Interfaces.Domain.Dictionary.Words;

public interface IWordsQueryResolver
{
    Task<List<LanguageWord>> GetAll(LanguageId languageId);
}