namespace IntervalLearningApi.Models.Dictonary;

public class LanguageDto
{
    public string Id { get; }
    public string Name { get; }

    public LanguageDto(short id, string name)
    {
        Id = id.ToString();
        Name = name;
    }
}