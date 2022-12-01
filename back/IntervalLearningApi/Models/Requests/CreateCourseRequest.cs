namespace IntervalLearningApi.Models.Requests;

public record CreateCourseRequest(string Name, string Description, bool IsPrivate);