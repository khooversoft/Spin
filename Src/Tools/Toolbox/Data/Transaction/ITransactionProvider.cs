namespace Toolbox.Data;

public interface ITransactionProvider
{
    string Name { get; }
    ITransaction CreateTransaction();
}
