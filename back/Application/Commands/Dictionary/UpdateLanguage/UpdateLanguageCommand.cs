using Domain.Common.ValueObjects;
using Domain.Language;
using DomainServices.DB.Queries.Dictionary;
using DomainServices.DB.Repositories;
using FluentResults;
using FluentResults.Extensions;
using GlobalTools.Errors;
using GlobalTools.Extensions;

namespace Application.Commands.Dictionary.UpdateLanguage;

public class UpdateLanguageCommand : ICommand<UpdateLanguageRequest, Language>
{
    private readonly IDictionaryQueryRepository dictionaryQueryRepository;
    private readonly IRepository<Language> languagesRepository;

    public UpdateLanguageCommand(
        IDictionaryQueryRepository dictionaryQueryRepository,
        IRepository<Language> languagesRepository)
    {
        this.dictionaryQueryRepository = dictionaryQueryRepository;
        this.languagesRepository = languagesRepository;
    }

    public async Task<Result<Language>> Handle(UpdateLanguageRequest request)
    {
        var language = await dictionaryQueryRepository.Languages.Find(request.Id);

        if (language is null)
            return Result.Fail(new NotFoundError(nameof(Language)));

        var nameResult = ShortString.Create(request.Name);
        if (nameResult.IsFailed) return Result.Fail<Language>(nameResult.Errors);

        var nativeNameResult = ShortString.Create(request.NativeLanguageName);
        if (nativeNameResult.IsFailed) return Result.Fail<Language>(nativeNameResult.Errors);

        language.Name = nameResult.Value;
        language.NativeLanguageName = nativeNameResult.Value;
        language.TranslationLink = request.TranslationLink;

        if (!string.IsNullOrEmpty(request.TranslationLinkTitle))
        {
            var titleResult = ShortString.Create(request.TranslationLinkTitle);
            if (titleResult.IsFailed) return Result.Fail<Language>(titleResult.Errors);
            language.TranslationLinkTitle = titleResult.Value;
        }
        else
        {
            language.TranslationLinkTitle = null;
        }

        return languagesRepository.UpdateAndSave(language);
    }
}
