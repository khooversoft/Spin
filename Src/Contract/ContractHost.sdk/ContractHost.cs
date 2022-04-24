using Bank.Abstractions;

namespace ContractHost.sdk;

public class ContractHost : IContractHost
{

    public ContractHost()
    {
    }

    public IServiceProvider ServiceProvider { get; init; } = null!;

    public IBankServices BankServices { get; init; } = null!;

    public IContractService ContractService { get; init; } = null!;

    public IFinancialService FinancialService { get; init; } = null!;

    public async Task Run()
    {
    }
}
