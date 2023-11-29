using Application.Common.Interfaces.Domain.Languages;
using Domain.Language;
using Domain.Language.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace DB.Resolvers.Languages;

public class LanguagesQueryResolver : ILanguagesQueryResolver
{
    private readonly ApplicationContext db;

    public LanguagesQueryResolver(ApplicationContext db)
    {
        this.db = db;
    }

    public Task<Language?> Find(LanguageId languageId)
    {
        return db.Languages.FindAsync(languageId).AsTask();
    }

    public Task<List<Language>> GetAll()
    {
        return db.Languages.ToListAsync();
    }
}