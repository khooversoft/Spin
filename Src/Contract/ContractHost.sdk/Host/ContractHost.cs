using ContractHost.sdk.Event;
using ContractHost.sdk.Model;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Toolbox.Abstractions.Tools;
using Toolbox.Extensions;
using Toolbox.Tools;

namespace ContractHost.sdk.Host;

public interface IContractHost
{
    IServiceProvider ServiceProvider { get; }
    ContractContext Context { get; }

    Task Run();
}


public class ContractHost : IContractHost
{
    private readonly ILogger<ContractHost> _logger;
    private readonly IRouter<string, Task> _router;

    public ContractHost(IServiceProvider serviceProvider, ContractContext contractContext, IRouter<string, Task> router, ILogger<ContractHost> logger)
    {
        ServiceProvider = serviceProvider.NotNull();
        Context = contractContext.NotNull();
        _router = router.NotNull();
        _logger = logger.NotNull();
    }

    public IServiceProvider ServiceProvider { get; }

    public ContractContext Context { get; }

    public async Task Run()
    {
        ContractHostOption option = ServiceProvider.GetRequiredService<ContractHostOption>().Verify();
        _logger.LogInformation("Starting eventName={eventPath}", Context.Option.EventPath);

        var tokenSource = new CancellationTokenSource();
        await _router.Send(Context.Option.EventPath, Context.Option.EventPath, tokenSource.Token);
    }
}

