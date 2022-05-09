using ContractHost.sdk.Event;

namespace ContractHost.sdk.Host;

public delegate Task EventNameHandler<T>(T service, IContractHost host, CancellationToken token) where T : class, IEventService;

public interface IContractHostBuilder
{
    IContractHostBuilder AddCommand(string[] args);
    IContractHostBuilder AddEvent<T>() where T : class, IEventService;
    IContractHostBuilder AddEvent<T>(EventName contractEvent, EventNameHandler<T> method) where T : class, IEventService;
    IContractHostBuilder AddSingleton<T>() where T : class;
    IContractHostBuilder AddSingleton<T>(Func<IServiceProvider, T> implementationFactory) where T : class;
    Task<IContractHost> Build();
}
