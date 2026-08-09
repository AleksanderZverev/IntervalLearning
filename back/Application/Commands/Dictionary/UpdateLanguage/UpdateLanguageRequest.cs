using Domain.Language.ValueObjects;

namespace Application.Commands.Dictionary.UpdateLanguage;

public record UpdateLanguageRequest(
    LanguageId Id,
    string Name,
    string NativeLanguageName,
    string? TranslationLink,
    string? TranslationLinkTitle
);
