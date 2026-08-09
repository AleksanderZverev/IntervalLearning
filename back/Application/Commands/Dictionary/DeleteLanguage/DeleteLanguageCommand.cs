using Domain.Language;
using DomainServices.DB.Queries.Dictionary;
using DomainServices.DB.Repositories;
using FluentResults;
using FluentResults.Extensions;
using GlobalTools.Errors;
using GlobalTools.Extensions;

namespace Application.Commands.Dictionary.DeleteLanguage;

public class DeleteLanguageCommand : ICommand<DeleteLanguageRequest>
{
    private readonly IDictionaryQueryRepository dictionaryQueryRepository;
    private readonly IRepository<Language> languagesRepository;

    public DeleteLanguageCommand(
        IDictionaryQueryRepository dictionaryQueryRepository,
        IRepository<Language> languagesRepository)
    {
        this.dictionaryQueryRepository = dictionaryQueryRepository;
        this.languagesRepository = languagesRepository;
    }

    public async Task<Result> Handle(DeleteLanguageRequest request)
    {
        return await dictionaryQueryRepository.Languages.Find(request.Id)
            .ToResultAsync()
            .ErrorIfNull(new NotFoundError(nameof(Language)))
            .Bind(language => languagesRepository.DeleteAndSave(language).ToResult());
    }
}
