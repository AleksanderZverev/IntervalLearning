namespace Application.Common.Interfaces.DB.Transactions;

public interface ITransactionScope : IDisposable
{
    void Complete();
}