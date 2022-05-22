using System.Linq;
using DB;
using DB.Models.Dictionary;
using Infrastructure;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace IntervalLearningApi.Services.Dictionary
{
    public class DictionaryService
    {
        private readonly ApplicationContext db;

        public DictionaryService(ApplicationContext db)
        {
            this.db = db;
        }

        public async Task<(List<TranslationEntity>? translations, string? error)> GetTranslations(long userId, string word)
        {
            var metadata = await db.UserMetadata.FindAsync(userId);

            if (metadata == null)
                throw new InvalidOperationException("User metadata not found");

            var lowerWord = word.ToLowerInvariant();
            var words = await db.Words.Where(w => w.Word.StartsWith(lowerWord)).ToListAsync();

            if (words.Count > 1)
            {
                return (null, "Many words found");
            }

            if (words.Count == 0)
                return (new List<TranslationEntity>(0), null);

            var foundWord = words[0];

            var translations = await db.Translations
                .Where(t => t.WordId == foundWord.Id && t.LanguageId == metadata.SuggestTranslationLanguageId)
                .ToListAsync();

            return (translations, null);
        }
    }
}
