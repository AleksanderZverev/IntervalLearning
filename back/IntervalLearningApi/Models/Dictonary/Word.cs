namespace IntervalLearningApi.Models.Dictonary
{
    public class WordDto
    {
        public int Id { get; }
        public string Word { get; }
        public string Pronunciation { get; }
        public short LanguageId { get; }

        public WordDto(int id, string word, string pronunciation, short languageId)
        {
            Id = id;
            Word = word;
            Pronunciation = pronunciation;
            LanguageId = languageId;
        }
    }
}
