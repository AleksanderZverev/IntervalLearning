using Domain.Common.ValueObjects;
using Domain.Language.ValueObjects;
using FluentResults;

namespace Domain.Language;

public class Language : Entity<LanguageId>
{
    private Language() : base()
    {
        //for EF
    }

    protected Language(
        LanguageId id,
        ShortString name,
        ShortString nativeLanguageName,
        ShortString? translationLinkTitle,
        string? translationLink)
    {
        Id = id;
        Name = name;
        NativeLanguageName = nativeLanguageName;
        TranslationLinkTitle = translationLinkTitle;
        TranslationLink = translationLink;
    }

    public LanguageId Id { get; private set; }
    public ShortString Name { get; private set; }
    public ShortString NativeLanguageName { get; private set; }

    public ShortString? TranslationLinkTitle { get; private set; }
    public string? TranslationLink { get; private set; }

    public static Result<Language> Create(
        short id,
        string name,
        string nativeLanguageName,
        string? translationLink = null,
        string? translationLinkTitle = null)
    {
        var idResult = LanguageId.Create(id);
        if (idResult.IsFailed) return idResult.ToResult();

        var nameResult = ShortString.Create(name);
        if (nameResult.IsFailed) return nameResult.ToResult();

        var nativeLanguageNameResult = ShortString.Create(nativeLanguageName);
        if (nativeLanguageNameResult.IsFailed) return nativeLanguageNameResult.ToResult();
        
        var translationLinkTitleResult = string.IsNullOrEmpty(translationLinkTitle) ? null : ShortString.Create(translationLinkTitle);
        if (translationLinkTitleResult is { IsFailed: true }) return translationLinkTitleResult.ToResult();

        return new Language(
            idResult.Value,
            nameResult.Value,
            nativeLanguageNameResult.Value,
            translationLinkTitleResult?.Value,
            translationLink);
    }
}