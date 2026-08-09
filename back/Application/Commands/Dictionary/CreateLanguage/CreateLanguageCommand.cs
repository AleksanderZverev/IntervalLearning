using Domain.Language;
using DomainServices.DB.Repositories;
using FluentResults;

namespace Application.Commands.Dictionary.CreateLanguage;

public class CreateLanguageCommand : ICommand<CreateLanguageRequest, Language>
{
    private readonly IRepository<Language> languagesRepository;

    public CreateLanguageCommand(IRepository<Language> languagesRepository)
    {
        this.languagesRepository = languagesRepository;
    }

    public Task<Result<Language>> Handle(CreateLanguageRequest request)
    {
        var languageResult = Language.Create(
            0,
            request.Name,
            request.NativeLanguageName,
            request.TranslationLink,
            request.TranslationLinkTitle);

        if (languageResult.IsFailed)
            return Task.FromResult(languageResult);

        return Task.FromResult(languagesRepository.AddAndSave(languageResult.Value));
    }
}
