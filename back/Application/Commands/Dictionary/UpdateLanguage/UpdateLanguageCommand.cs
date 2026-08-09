using Domain.Language;
using DomainServices.DB.Queries.Dictionary;
using DomainServices.DB.Repositories;
using FluentResults;
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

        var updateResult = language.Update(
            request.Name,
            request.NativeLanguageName,
            request.TranslationLink,
            request.TranslationLinkTitle);

        if (updateResult.IsFailed)
            return Result.Fail<Language>(updateResult.Errors);

        return await languagesRepository.UpdateAndSave(language);
    }
}
