namespace IntervalLearningApi.IntegrationTests.Learning.Common;

public class LearningCommons
{
    public static IReadOnlyList<TimeSpan> phasesDuration = new List<TimeSpan>()
    {
        TimeSpan.FromDays(1),
            
        TimeSpan.FromDays(3),
            
        TimeSpan.FromDays(7),
            
        TimeSpan.FromDays(14),
        TimeSpan.FromDays(1),
            
        TimeSpan.FromDays(28),
            
        TimeSpan.FromDays(28),
            
        TimeSpan.FromDays(40),
    };
    
    public static IReadOnlyList<TimeSpan> phasesDurationWithRepetitions = new List<TimeSpan>()
    {
        TimeSpan.FromDays(1),
        TimeSpan.FromSeconds(1),
            
        TimeSpan.FromDays(3),
        TimeSpan.FromSeconds(1),
            
        TimeSpan.FromDays(7),
        TimeSpan.FromSeconds(1),
            
        TimeSpan.FromDays(14),
        TimeSpan.FromSeconds(1),
        
        TimeSpan.FromDays(1),
        TimeSpan.FromSeconds(1),
            
        TimeSpan.FromDays(28),
        TimeSpan.FromSeconds(1),
            
        TimeSpan.FromDays(28),
        TimeSpan.FromSeconds(1),
            
        TimeSpan.FromDays(40),
        TimeSpan.FromSeconds(1),
    };
}