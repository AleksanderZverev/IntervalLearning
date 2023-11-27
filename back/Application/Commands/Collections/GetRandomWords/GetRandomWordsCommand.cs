using Application.Common.Interfaces.Domain.Cards;
using Application.Common.Interfaces.Domain.Collections;
using Application.Common.Interfaces.Domain.Dictionary.Words;
using Application.Common.Interfaces.Domain.Languages;
using Application.Common.Interfaces.Domain.Themes;
using FluentResults;
using Infrastructure;
using Infrastructure.Errors;

namespace Application.Commands.Collections.GetRandomWords;

public class GetRandomWordsCommand : ICommand<GetRandomWordsRequest, GetRandomWordsResponse>
{
    private readonly ICollectionQueryResolver collectionQueryResolver;
    private readonly IThemesQueryResolver themesQueryResolver;
    private readonly ICardsQueryResolver cardsQueryResolver;
    private readonly ILanguagesQueryResolver languagesQueryResolver;
    private readonly IWordsQueryResolver wordsQueryResolver;

    public GetRandomWordsCommand(
        ICollectionQueryResolver collectionQueryResolver,
        IThemesQueryResolver themesQueryResolver,
        ICardsQueryResolver cardsQueryResolver,
        ILanguagesQueryResolver languagesQueryResolver,
        IWordsQueryResolver wordsQueryResolver)
    {
        this.collectionQueryResolver = collectionQueryResolver;
        this.themesQueryResolver = themesQueryResolver;
        this.cardsQueryResolver = cardsQueryResolver;
        this.languagesQueryResolver = languagesQueryResolver;
        this.wordsQueryResolver = wordsQueryResolver;
    }

    public async Task<Result<GetRandomWordsResponse>> Handle(GetRandomWordsRequest request)
    {
        var (userId, collectionId) = request;
        
        var collection = await collectionQueryResolver.Find(userId, collectionId);

        if (collection == null)
        {
            return new NotFoundError("Collection");
        }

        var theme = await themesQueryResolver.Find(collection.ThemeId);

        if (theme?.LanguageId == null)
        {
            return new BadRequestError(theme == null
                ? "Theme not found"
                : "Language not linked");
        }

        var language = await languagesQueryResolver.Find(theme.LanguageId);

        if (language == null)
            return new NotFoundError("Language");

        var words = await wordsQueryResolver.GetAll(theme.LanguageId);
        words.Shuffle();

        var userCollections = await collectionQueryResolver.GetAll(userId);
        var collectionIds = userCollections
            .Where(c => c.ThemeId == theme.Id)
            .Select(c => c.Id)
            .Distinct()
            .ToList();

        var cards = await cardsQueryResolver.GetRangeFromCollections(userId, collectionIds);

        var resultWords = words
            .Where(w => !cards.Exists(c => 
                string.Equals(c.RememberingText, w.Word, StringComparison.InvariantCultureIgnoreCase)))
            .Take(30)
            .ToList();

        return new GetRandomWordsResponse(resultWords, language);
    }
}