using System.Transactions;
using DomainServices.DB.Transactions;

namespace DB.Transactions;

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