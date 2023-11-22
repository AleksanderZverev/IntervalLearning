namespace IntervalLearningApi.Interfaces.DbTransactions;

public interface ITransactionProvider
{
    ITransactionScope CreateScope();
}