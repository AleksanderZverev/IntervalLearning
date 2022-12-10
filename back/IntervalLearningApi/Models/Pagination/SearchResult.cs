namespace IntervalLearningApi.Models.Pagination;

public class SearchResult<TResult>
{
    public List<TResult>? FoundItems { get; set; }
    public int TotalCount { get; set; }
}