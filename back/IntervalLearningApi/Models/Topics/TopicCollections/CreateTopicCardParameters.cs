namespace IntervalLearningApi.Models.Topics.TopicCollections;

public record CreateTopicCardParameters(
    string RememberingText,
    string PromptText,
    string MeaningText,
    string Description,
    List<string> Examples);