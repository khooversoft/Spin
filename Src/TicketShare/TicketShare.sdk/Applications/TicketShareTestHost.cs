using Toolbox.Graph;
using Toolbox.Types;

namespace TicketShare.sdk;

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
