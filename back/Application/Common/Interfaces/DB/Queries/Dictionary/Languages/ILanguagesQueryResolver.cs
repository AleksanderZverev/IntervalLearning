using Domain.Language;
using Domain.Language.ValueObjects;

namespace Application.Common.Interfaces.DB.Queries.Dictionary.Languages;

public interface ILanguagesQueryResolver
{
    Task<Language?> Find(LanguageId languageId);
    Task<List<Language>> GetAll();
}