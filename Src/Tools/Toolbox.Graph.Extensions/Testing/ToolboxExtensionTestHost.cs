using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Toolbox.Extensions;
using Toolbox.Types;

namespace Toolbox.Graph.Extensions.Testing;

public class ToolboxExtensionTestHost
{
    private GraphTestClient _testClient;

    public ToolboxExtensionTestHost(string? principalId = null)
    {
        _testClient = GraphTestStartup.CreateGraphTestHost(null, service =>
        {
            service.AddToolboxIdentity();
            service.AddSingleton<AuthenticationStateProvider>(s =>
            {
                return principalId.IsEmpty() ? new TestAuthStateProvider() : new TestAuthStateProvider(principalId);
            });
        });
    }

    public IGraphClient TestClient => _testClient;

    public IServiceProvider ServiceProvider => _testClient.ServiceProvider;
    public ScopeContext GetScopeContext<T>() => _testClient.GetScopeContext<T>();
}
