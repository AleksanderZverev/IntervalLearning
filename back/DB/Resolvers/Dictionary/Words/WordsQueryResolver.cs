using Application.Common.Interfaces.Domain.Dictionary.Words;
using DB.Models.Dictionary;
using DB.Models.Dictionary.ValueObjects;
using Domain.Language.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace DB.Resolvers.Dictionary.Words;

public class WordsQueryResolver : IWordsQueryResolver
{
    private readonly ApplicationContext db;

    public WordsQueryResolver(ApplicationContext db)
    {
        this.db = db;
    }


    public Task<List<LanguageWord>> GetAll(LanguageId languageId)
    {
        return db.Words.Where(w => w.LanguageId == languageId).ToListAsync();
    }

    public Task<List<LanguageWord>> SearchWord(WordText text, int count)
    {
        var lowerWord = text.Value.ToLowerInvariant();
        return db.Words
            .Where(w => EF.Functions.ILike(w.Word, $"{lowerWord}%"))
            .Take(count)
            .ToListAsync();
    }

    public Task<List<LanguageWord>> SearchWordByPronunciation(WordText text, int count)
    {
        var lowerPronounce = text.Value.ToLowerInvariant();
        return db.Words.Where(w => w.Pronunciation != null &&  EF.Functions.ILike(w.Pronunciation, $"{lowerPronounce}%"))
            .Take(count)
            .ToListAsync();
    }
}