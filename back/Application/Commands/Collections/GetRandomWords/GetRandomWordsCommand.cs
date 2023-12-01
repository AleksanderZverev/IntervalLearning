using Application.Common.Interfaces.DB.Queries.Dictionary;
using Application.Common.Interfaces.DB.Repositories.Study;
using FluentResults;
using Infrastructure;
using Infrastructure.Errors;

namespace Application.Commands.Collections.GetRandomWords;

public class GetRandomWordsCommand : ICommand<GetRandomWordsRequest, GetRandomWordsResponse>
{
    private readonly IStudyQueryRepository studyQueryRepository;
    private readonly IDictionaryQueryRepository dictionaryQueryRepository;

    public GetRandomWordsCommand(
        IStudyQueryRepository studyQueryRepository,
        IDictionaryQueryRepository dictionaryQueryRepository)
    {
        this.studyQueryRepository = studyQueryRepository;
        this.dictionaryQueryRepository = dictionaryQueryRepository;
    }

    public async Task<Result<GetRandomWordsResponse>> Handle(GetRandomWordsRequest request)
    {
        var (userId, collectionId) = request;
        
        var collection = await studyQueryRepository.Collections.Find(userId, collectionId);

        if (collection == null)
        {
            return new NotFoundError("Collection");
        }

        var theme = await studyQueryRepository.Themes.Find(collection.ThemeId);

        if (theme?.LanguageId == null)
        {
            return new BadRequestError(theme == null
                ? "Theme not found"
                : "Language not linked");
        }

        var language = await dictionaryQueryRepository.Languages.Find(theme.LanguageId);

        if (language == null)
            return new NotFoundError("Language");

        var words = await dictionaryQueryRepository.Words.GetAll(theme.LanguageId);
        words.Shuffle();

        var userCollections = await studyQueryRepository.Collections.GetAll(userId);
        var collectionIds = userCollections
            .Where(c => c.ThemeId == theme.Id)
            .Select(c => c.Id)
            .Distinct()
            .ToList();

        var cards = await studyQueryRepository.Cards.GetRangeFromCollections(userId, collectionIds);

        var resultWords = words
            .Where(w => !cards.Exists(c => 
                string.Equals(c.RememberingText, w.Word, StringComparison.InvariantCultureIgnoreCase)))
            .Take(30)
            .ToList();

        return new GetRandomWordsResponse(resultWords, language);
    }
}