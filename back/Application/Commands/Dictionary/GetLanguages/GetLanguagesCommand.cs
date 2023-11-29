using Application.Common.Interfaces.DB.Queries.Dictionary;
using Application.Common.Interfaces.Domain.Languages;
using Domain.Language;
using FluentResults;
using Infrastructure.Extensions;

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