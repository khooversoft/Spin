using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Toolbox.Graph;
using Toolbox.Tools;
using Toolbox.Logging;
using Toolbox.Types;

namespace KGraphCmd.Application;

public class GraphHostManager : IAsyncDisposable
{
    private readonly ILogger<GraphHostManager> _logger;
    private readonly object _lock = new object();
    private ServiceProvider? _serviceProvider;

    public GraphHostManager(ILogger<GraphHostManager> logger) => _logger = logger.NotNull();

    public ServiceProvider ServiceProvider => _serviceProvider.NotNull();

    public async ValueTask DisposeAsync() => await Close();

    public async ValueTask Close()
    {
        if (_serviceProvider == null) return;
        await _serviceProvider.DisposeAsync();
        _serviceProvider = null;
    }

    public IServiceProvider Start(string jsonFile)
    {
        lock (_lock)
        {
            _serviceProvider?.Dispose();
            _serviceProvider = HostTool.StartHost(jsonFile);
            return _serviceProvider;
        }
    }

    public async Task LoadMap(ScopeContext context)
    {
        var client = _serviceProvider.NotNull().GetRequiredService<IGraphHost>();
        context.LogInformation("Loading map...");
        var result = await client.LoadMap(context);
        result.LogStatus(context, "Load map result");
    }
}
