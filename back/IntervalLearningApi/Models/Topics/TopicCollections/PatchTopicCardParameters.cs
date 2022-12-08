namespace IntervalLearningApi.Models.Topics.TopicCollections;

public record PatchTopicCardParameters(
    string RememberingText,
    string PromptText,
    string MeaningText,
    string Description,
    List<string> Examples);