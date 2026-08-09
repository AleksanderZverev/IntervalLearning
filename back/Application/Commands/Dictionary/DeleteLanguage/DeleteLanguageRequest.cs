using Domain.Language.ValueObjects;

namespace Application.Commands.Dictionary.DeleteLanguage;

public record DeleteLanguageRequest(LanguageId Id);
