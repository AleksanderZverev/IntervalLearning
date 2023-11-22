using System.Transactions;
using IntervalLearningApi.Interfaces.DbTransactions;

namespace IntervalLearningApi.Infrastructure.DbTransactions;

public class TransactionProvider : ITransactionProvider
{
    public ITransactionScope CreateScope()
    {
        var transaction = new TransactionScope(
            TransactionScopeOption.Required,
            new TransactionOptions()
            {
                IsolationLevel = IsolationLevel.ReadUncommitted,
                Timeout = TimeSpan.FromMinutes(5),
            },
            TransactionScopeAsyncFlowOption.Enabled);
        
        return new TransactionScopeWrapper(transaction);
    }
}