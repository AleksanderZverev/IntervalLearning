namespace Application.Commands.Dictionary.CreateLanguage;

public record CreateLanguageRequest(
    string Name,
    string NativeLanguageName,
    string? TranslationLink,
    string? TranslationLinkTitle
);
