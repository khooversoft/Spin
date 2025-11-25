namespace Toolbox.Data;

public interface ITransactionRegister
{
    public DataChangeRecorder DataChangeLog { get; }
    public ITransactionProvider GetProvider();
}