namespace IntervalLearningApi.Models.Dictonary;

public class LanguageDto
{
    public short Id { get; }
    public string Name { get; }

    public LanguageDto(short id, string name)
    {
        Id = id;
        Name = name;
    }
}