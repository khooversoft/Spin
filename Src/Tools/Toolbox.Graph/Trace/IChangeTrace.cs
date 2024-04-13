namespace Toolbox.Graph;

public interface IChangeTrace
{
    void Log(ChangeTrx trx);
    Task LogAsync(ChangeTrx trx);
}

