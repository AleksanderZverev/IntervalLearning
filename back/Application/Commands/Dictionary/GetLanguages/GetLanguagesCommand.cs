using Application.Common.Interfaces.Domain.Languages;
using Domain.Language;
using FluentResults;
using Infrastructure.Extensions;

namespace Application.Commands.Dictionary.GetLanguages;

public class GetLanguagesCommand : ICommand<GetLanguagesRequest, List<Language>>
{
    private readonly ILanguagesQueryResolver languagesQueryResolver;

    public GetLanguagesCommand(
        ILanguagesQueryResolver languagesQueryResolver)
    {
        this.languagesQueryResolver = languagesQueryResolver;
    }

    public Task<Result<List<Language>>> Handle(GetLanguagesRequest request)
    {
        return languagesQueryResolver.GetAll().ToResultAsync();
    }
}