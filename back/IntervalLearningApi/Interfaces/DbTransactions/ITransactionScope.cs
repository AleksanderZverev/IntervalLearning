namespace IntervalLearningApi.Interfaces.DbTransactions;

public interface ITransactionScope : IDisposable
{
    void Complete();
}