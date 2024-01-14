using System.Transactions;
using DomainServices.DB.Transactions;

namespace DB.Transactions;

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