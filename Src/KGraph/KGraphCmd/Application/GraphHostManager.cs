using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Toolbox.Tools;

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
}
