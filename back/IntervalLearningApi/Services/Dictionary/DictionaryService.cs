using DB;
using Domain.Dictionary.Translation;
using Domain.Dictionary.Translation.ValueObjects;
using Domain.Dictionary.Word;
using Domain.Dictionary.Word.ValueObjects;
using Domain.Language.ValueObjects;
using Domain.User.ValueObjects;
using DomainServices.DB.Transactions;
using FluentResults;
using GlobalTools;
using GlobalTools.Errors;
using Microsoft.EntityFrameworkCore;

namespace IntervalLearningApi.Services.Dictionary
{
    public class DictionaryService
    {
        private readonly ITransactionProvider transactionProvider;
        private readonly ApplicationContext db;
        private readonly IHostEnvironment env;

        public DictionaryService(
            ITransactionProvider transactionProvider,
            ApplicationContext db,
            IHostEnvironment env)
        {
            this.transactionProvider = transactionProvider;
            this.db = db;
            this.env = env;
        }

        public async Task<Result<List<WordTranslation>>> GetTranslations(UserId userId, string word)
        {
            var metadata = await db.UserMetadata.FindAsync(userId);

            if (metadata == null)
                throw new InvalidOperationException("User metadata not found");

            var lowerWord = word.Trim().ToLowerInvariant();
            var words = await db.Words.Where(w => w.Word == lowerWord).ToListAsync();

            if (words.Count > 1)
            {
                return new BadRequestError("Found more than 1 word");
            }

            if (words.Count == 0)
                return new List<WordTranslation>();

            var foundWord = words[0];

            var translations = await db.Translations
                .Where(t => t.WordId == foundWord.Id && t.LanguageId == metadata.SuggestTranslationLanguageId)
                .ToListAsync();

            return translations;
        }

        public async Task<Result<string>> ParseWordsWithTranslations(
            UserId userId,
            short languageId,
            short translationLanguageId,
            string text)
        {
            var user = await db.Users.FindAsync(userId);

            var lines = text.Split("\n", StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            var errors = new List<string>(5);

            var allWords = await db.Words.ToListAsync();
            var wordIdToTranslations = new Dictionary<int, List<WordTranslation>>();

            using var transaction = transactionProvider.CreateScope();

            foreach (var line in lines)
            {
                var split = line.Split('\t', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

                if (split.Length != 3)
                {
                    errors.Add(line + " - incorrect format");
                    continue;
                }

                var wordText = WordText.Create(split[0]).Value;
                var pronunciation = WordPronunciation.Create(split[1]).Value;
                var translationsLine = split[2];

                var word = allWords.FirstOrDefault(w => string.Equals(w.Word, wordText, StringComparison.InvariantCultureIgnoreCase));

                if (word != null && string.IsNullOrEmpty(word.Pronunciation))
                {
                    word.Pronunciation = pronunciation;

                    if (!db.SoftSaveChanges())
                    {
                        errors.Add(line + " - on update pronunciation");
                    }
                }

                if (word == null)
                {
                    word = new LanguageWord
                    {
                        LanguageId = LanguageId.Create(languageId).Value,
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
                    wordIdToTranslations.Add(word.Id, new List<WordTranslation>(translationsFromDb));

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
                        translationErrors.Add("[already exists] " + translationText);
                        continue;
                    }

                    short id;
                    var maxTries = 100;
                    var count = 0;
                    var containsSameId = false;
                    do
                    {
                        id = RandomMaster.GenerateShort();
                        containsSameId = translations.FirstOrDefault(t => t.Id == id) != null;
                        count++;
                    } while (count < maxTries && containsSameId);

                    if (containsSameId)
                    {
                        translationErrors.Add("[unable to generate id] " + translationText);
                        continue;
                    }

                    var translation = new WordTranslation()
                    {
                        Id = id,
                        LanguageId = LanguageId.Create(translationLanguageId).Value,
                        Translation = TranslationText.Create(lowerTranslation).Value,
                        WordId = word.Id,
                    };

                    db.Entry(translation).State = EntityState.Added;

                    var isAdded = db.SoftSaveChanges();

                    if (isAdded)
                        translations.Add(translation);
                    else
                        translationErrors.Add("[error on add] " + translationText);
                }

                if (translationErrors.Count > 0)
                {
                    errors.Add(line + "\n\t" + string.Join("\n\t", translationErrors));
                }
            }

            transaction.Complete();
            return string.Join("\n\n", errors);
        }
    }
}
