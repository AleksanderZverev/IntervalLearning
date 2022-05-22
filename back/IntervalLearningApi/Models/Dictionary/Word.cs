namespace IntervalLearningApi.Models.Dictionary
{
    public class WordDto
    {
        public string Id { get; }
        public string Word { get; }
        public string? Pronunciation { get; }
        public string LanguageId { get; }

        public WordDto(int id, string word, string? pronunciation, short languageId)
        {
            Id = id.ToString();
            Word = word;
            Pronunciation = pronunciation;
            LanguageId = languageId.ToString();
        }
    }
}
