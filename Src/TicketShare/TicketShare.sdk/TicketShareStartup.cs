using Microsoft.Extensions.DependencyInjection;
using Toolbox.Graph;
using Toolbox.Identity;
using Toolbox.Types;

namespace TicketShare.sdk;

public static class TicketShareStartup
{
    public static IServiceCollection AddTicketShare(this IServiceCollection service)
    {
        service.AddIdentity();
        service.AddSingleton<AccountClient>();

        service.AddSingleton<TicketGroupClient>();
        service.AddSingleton<TicketGroupProposalClient>();
        service.AddSingleton<TicketGroupSearchClient>();

        service.AddSingleton<HubChannelClient>();
        return service;
    }
}

public class TicketShareTestHost
{
    private GraphTestClient _testClient;

    public TicketShareTestHost()
    {
        _testClient = GraphTestStartup.CreateGraphTestHost(null, service =>
        {
            service.AddTicketShare();
        });
    }

    public IGraphClient TestClient => _testClient;

    public IServiceProvider ServiceProvider => _testClient.ServiceProvider;
    public ScopeContext GetScopeContext<T>() => _testClient.GetScopeContext<T>();
}
