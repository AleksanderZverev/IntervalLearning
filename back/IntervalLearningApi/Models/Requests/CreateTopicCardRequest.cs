namespace IntervalLearningApi.Models.Requests;

public record CreateTopicCardRequest(
    string RememberingText,
    string PromptText,
    string MeaningText,
    string Description,
    List<string> Examples);