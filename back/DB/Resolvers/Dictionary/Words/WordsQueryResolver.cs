using Application.Common.Interfaces.Domain.Dictionary.Words;
using DB.Models.Dictionary;
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
}