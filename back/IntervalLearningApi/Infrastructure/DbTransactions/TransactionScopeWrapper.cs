using System.Transactions;
using IntervalLearningApi.Interfaces.DbTransactions;

namespace IntervalLearningApi.Infrastructure.DbTransactions;

public class TransactionScopeWrapper : ITransactionScope
{
    private readonly TransactionScope transactionScope;

    public TransactionScopeWrapper(TransactionScope transactionScope)
    {
        this.transactionScope = transactionScope;
    }

    public void Complete()
    {
        transactionScope.Complete();
    }

    public void Dispose()
    {
        transactionScope.Dispose();
    }
}