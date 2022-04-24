namespace ContractHost.sdk;

public interface IStateService
{
    Task Run(IContractHost runHost);
}
