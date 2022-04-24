namespace ContractHost.sdk;

public interface IContractHost
{
    IServiceProvider ServiceProvider { get; }

    public IBankServices BankServices { get; }

    public IContractService ContractService { get; }

    public IFinancialService FinancialService { get; }

    Task Run();
}
