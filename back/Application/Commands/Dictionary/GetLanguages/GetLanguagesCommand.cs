using Domain.Language;
using DomainServices.DB.Queries.Dictionary;
using FluentResults;
using GlobalTools.Extensions;

namespace Application.Commands.Dictionary.GetLanguages;

public class GetLanguagesCommand : ICommand<GetLanguagesRequest, List<Language>>
{
    private readonly IDictionaryQueryRepository dictionaryQueryRepository;

    public GetLanguagesCommand(
        IDictionaryQueryRepository dictionaryQueryRepository)
    {
        this.dictionaryQueryRepository = dictionaryQueryRepository;
    }

    public Task<Result<List<Language>>> Handle(GetLanguagesRequest request)
    {
        return dictionaryQueryRepository.Languages.GetAll().ToResultAsync();
    }
}