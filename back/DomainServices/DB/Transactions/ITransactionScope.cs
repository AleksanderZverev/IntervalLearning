namespace DomainServices.DB.Transactions;

public interface ITransactionScope : IDisposable
{
    void Complete();
}