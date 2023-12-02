namespace IntervalLearningApi.Controllers.Study.Statistics.DTOs;

public record LearningStatisticModel(
    int RepeatedCards,
    int LearnedCards
);