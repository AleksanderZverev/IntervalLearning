namespace IntervalLearningApi.Models.Requests;

public record PatchTopicCardRequest(
    string RememberingText,
    string PromptText,
    string MeaningText,
    string Description,
    List<string> Examples);