using Domain.Language;
using Domain.Language.ValueObjects;

namespace Application.Common.Interfaces.Domain.Languages;

public interface ILanguagesQueryResolver
{
    Task<Language?> Find(LanguageId languageId);
    Task<List<Language>> GetAll();
}