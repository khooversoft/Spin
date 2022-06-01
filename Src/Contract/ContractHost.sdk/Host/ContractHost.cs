using ContractHost.sdk.Event;
using ContractHost.sdk.Model;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Toolbox.Extensions;
using Toolbox.Tools;

namespace ContractHost.sdk.Host;

public class ContractHost : IContractHost
{
    private readonly ILogger<ContractHost> _logger;

    public ContractHost(IServiceProvider serviceProvider, ContractContext contractContext, ILogger<ContractHost> logger)
    {
        ServiceProvider = serviceProvider.NotNull();
        Context = contractContext.NotNull();
        _logger = logger.NotNull();
    }

    public IServiceProvider ServiceProvider { get; }

    public ContractContext Context { get; }

    public IDictionary<string, object> Properties { get; } = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);

    public async Task Run()
    {
        ContractHostOption option = ServiceProvider.GetRequiredService<ContractHostOption>().Verify();
        _logger.LogInformation("Starting eventName={eventName}", Context.Option.EventName);

        var tokenSource = new CancellationTokenSource();

        foreach (EventClassRegistry eventClass in Context.EventClassRegistries.Where(x => x.EventName == Context.Option.EventName))
        {
            IEventService service = (IEventService)ServiceProvider.GetRequiredService(eventClass.Type);
            await eventClass.Method(service, this, tokenSource.Token);
        }
    }
}

