namespace IntervalLearningApi.Controllers.Study.Statistics.DTOs;

public record LearningStatisticModel(
    int TotalRepeatingCards,
    Dictionary<string, PhaseStatisticDto> PhaseIdToStatistic,
    int RepeatedCards,
    int LearnedCards
);

public class PhaseStatisticDto
{
    public required string PhaseId { get; set; }
    public required int TotalRepeatingCards { get; set; }
    public required int LateCards { get; set; }
    public required int TodayCards { get; set; }
    public required int FutureCards { get; set; }
}