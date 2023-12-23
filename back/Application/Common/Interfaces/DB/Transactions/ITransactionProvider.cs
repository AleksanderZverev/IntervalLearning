namespace Application.Common.Interfaces.DB.Transactions;

public interface ITransactionProvider
{
    ITransactionScope CreateScope();
}