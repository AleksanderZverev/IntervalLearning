namespace IntervalLearningApi.Controllers.Study.Statistics.DTOs;

public record CalendarLearningStatisticModel(
    int LearnedCards,
    Dictionary<DateTime, int> DateToLearnedCards,
    Dictionary<DateTime, int> DateToRepeatedCards,
    Dictionary<DateTime, int> DateQueueCards,
    Dictionary<DateTime, int> DateToRecommendationToLearn);