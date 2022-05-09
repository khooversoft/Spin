using ContractHost.sdk.Model;

namespace ContractHost.sdk.Host;

public interface IContractHost
{
    IServiceProvider ServiceProvider { get; }
    ContractContext Context { get; }
    IDictionary<string, object> Properties { get; }

    Task Run();
}
