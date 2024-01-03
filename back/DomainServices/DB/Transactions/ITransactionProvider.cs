namespace DomainServices.DB.Transactions;

public interface ITransactionProvider
{
    ITransactionScope CreateScope();
}