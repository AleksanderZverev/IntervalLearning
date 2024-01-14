using Domain.Language;
using Domain.Language.ValueObjects;

namespace DomainServices.DB.Queries.Dictionary.Languages;

public interface ILanguagesQueryResolver
{
    Task<Language?> Find(LanguageId languageId);
    Task<List<Language>> GetAll();
}