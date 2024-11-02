using Microsoft.Extensions.DependencyInjection;
using Toolbox.Graph;
using Toolbox.Identity;
using Toolbox.Types;

namespace TicketShare.sdk.Applications;

public class TestHost
{
    private GraphTestClient _testClient;

    public TestHost()
    {
        _testClient = GraphTestStartup.CreateGraphTestHost(null, service =>
        {
            service.AddSingleton<IdentityClient>();
            service.AddSingleton<AccountClient>();
            service.AddSingleton<TicketGroupClient>();
        });
    }

    public IGraphClient TestClient => _testClient;

    public IServiceProvider ServiceProvider => _testClient.ServiceProvider;
    public ScopeContext GetScopeContext<T>() => _testClient.GetScopeContext<T>();
}
