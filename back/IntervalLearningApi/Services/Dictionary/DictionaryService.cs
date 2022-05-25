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

            var lowerWord = word.Trim().ToLowerInvariant();
            var words = await db.Words.Where(w => w.Word == lowerWord).ToListAsync();

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

        public async Task<(string? okText, string? error)> ParseWordsWithTranslations(
            long userId, 
            short languageId, 
            short translationLanguageId, 
            string text)
        {
            var user = await db.Users.FindAsync(userId);

            if (user is not {Email: "sam998980@mail.ru"})
                return (null, "Forbidden");

            var lines = text.Split("\n", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            var errors = new List<string>(5);

            var allWords = await db.Words.ToListAsync();
            var wordIdToTranslations = new Dictionary<int, List<TranslationEntity>>();

            db.Database.BeginTransaction();

            foreach (var line in lines)
            {
                var split = line.Split('\t', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

                if (split.Length != 3)
                {
                    errors.Add(line + " - incorrect format");
                    continue;
                }

                var wordText = split[0].ToLowerInvariant();
                var pronunciation = split[1].ToLowerInvariant();
                var translationsLine = split[2];

                var word = allWords.FirstOrDefault(w => string.Equals(w.Word, wordText, StringComparison.InvariantCultureIgnoreCase));

                if (word != null && string.IsNullOrEmpty(word.Pronunciation))
                {
                    word.Pronunciation = pronunciation;

                    try
                    {
                        db.SaveChanges();
                    }
                    catch
                    {
                        errors.Add(line + " - on update pronunciation");
                    }
                }

                if (word == null)
                { 
                    word = new WordEntity
                    {
                        LanguageId = languageId,
                        Word = wordText,
                        Pronunciation = pronunciation
                    };

                    db.Entry(word).State = EntityState.Added;

                    try
                    {
                        db.SaveChanges();
                        allWords.Add(word);
                    }
                    catch
                    {
                        errors.Add(line + " - error on add");
                        continue;
                    }
                }

                var translationsFromDb = await db.Translations
                    .Where(t => t.LanguageId == translationLanguageId && t.WordId == word.Id)
                    .ToListAsync();

                if (!wordIdToTranslations.ContainsKey(word.Id))
                    wordIdToTranslations.Add(word.Id, new List<TranslationEntity>(translationsFromDb));

                var translations = wordIdToTranslations[word.Id];
                var translationsSplit = translationsLine.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

                var translationErrors = new List<string>();

                foreach (var translationText in translationsSplit)
                {
                    var lowerTranslation = translationText.ToLowerInvariant();

                    var sameTranslation = translations
                        .FirstOrDefault(t => 
                            string.Equals(t.Translation, lowerTranslation, StringComparison.InvariantCultureIgnoreCase));

                    if (sameTranslation != null)
                    {
                        translationErrors.Add(translationText);
                        continue;
                    }

                    var translation = new TranslationEntity()
                    {
                        LanguageId = translationLanguageId,
                        Translation = lowerTranslation,
                        WordId = word.Id,
                    };

                    db.Entry(translation).State = EntityState.Added;

                    try
                    {
                        db.SaveChanges();
                        translations.Add(translation);
                    }
                    catch
                    {
                        translationErrors.Add(translationText + " - [error on add]");
                    }
                }

                if (translationErrors.Count > 0)
                {
                    errors.Add(line + " - translationsErrors: " + string.Join(", ", translationErrors));
                }
            }

            db.Database.CommitTransaction();

            return (string.Join("\n\n", errors), null);
        }

        public async Task<List<LanguageEntity>> GetLanguages()
        {
            return await db.Languages.ToListAsync();
        }
    }
}
